using LibraryNetCoreAPI.DTO;
using LibraryNetCoreAPI.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CuentasController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly HashService hashService;
        private readonly IDataProtector dataProtector;

        public CuentasController(UserManager<IdentityUser> userManager, IConfiguration configuration, 
            SignInManager<IdentityUser> signInManager,
            IDataProtectionProvider dataProtectionProvider,
            HashService hashService) //hemos inyectado estos servicios necesarios
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.hashService = hashService;
            this.dataProtector = dataProtectionProvider.CreateProtector("string_secreto_de_proposito");
        }

        [HttpGet("hash/{textoPlano}")]
        public ActionResult AplicarHash(string textoPlano)
        {
            var resultado1 = hashService.Hash(textoPlano);
            var resultado2 = hashService.Hash(textoPlano);

            return Ok(
                new
                {
                    textoPlano = textoPlano,
                    hash1 = resultado1,
                    hash2 = resultado2
                }    
            );
        }

        [HttpGet("encriptar")]
        public ActionResult Encriptar()
        {
            var textoPlano = "Gerson Azabache Net Core";
            var textoCifrado = dataProtector.Protect(textoPlano);
            var textoDescifrado = dataProtector.Unprotect(textoCifrado);

            return Ok(
                new {
                    textoPlano = textoPlano,
                    textoCifrado = textoCifrado,
                    textoDescifrado = textoDescifrado
                }
            );
        }

        [HttpGet("encriptarConExpiracion")]
        public ActionResult EncriptarConExpiracion()
        {
            var textoPlano = "Gerson Azabache Net Core";
            var dataProtectorConExpiracion = dataProtector.ToTimeLimitedDataProtector();
            var textoCifrado = dataProtectorConExpiracion.Protect(textoPlano, lifetime: TimeSpan.FromSeconds(5));
            Thread.Sleep(5000);
            var textoDescifrado = dataProtectorConExpiracion.Unprotect(textoCifrado);

            return Ok(
                new
                {
                    textoPlano = textoPlano,
                    textoCifrado = textoCifrado,
                    textoDescifrado = textoDescifrado
                }
            );
        }

        [HttpPost("registrar", Name = "registrarUsuario")] //esto hace que la ruta sea api/cuentas/registrar
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Registrar(CredencialesUsuarioDTO credencialesUsuario)
        {
            var identityUser = new IdentityUser { UserName = credencialesUsuario.Email, Email = credencialesUsuario.Email };
            var resultado = await userManager.CreateAsync(identityUser, credencialesUsuario.Password);

            if (resultado.Succeeded)
            {
                //retorno del jwt
                return await ConstruirToken(credencialesUsuario);
            }
            else
            {
                return BadRequest(resultado.Errors);
            }
        }

        [HttpPost("login", Name = "loginUsuario")] //esto hace que la ruta sea api/cuentas/registrar
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login(CredencialesUsuarioDTO credencialesUsuario)
        {
            var resultado = await signInManager.PasswordSignInAsync(credencialesUsuario.Email, credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);
            if (resultado.Succeeded)
                return await ConstruirToken(credencialesUsuario);
            else
                return BadRequest("Credenciales incorrectas");
        }

        [HttpGet("renovarToken", Name = "renovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Renovar()
        {
            var emailClaim = HttpContext.User.Claims.Where(x => x.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;

            var credencialesUsuario = new CredencialesUsuarioDTO()
            {
                Email = email
            };
            return await ConstruirToken(credencialesUsuario);
        }

        private async Task<RespuestaAutenticacionDTO> ConstruirToken(CredencialesUsuarioDTO credencialesUsuario)
        {
            //creamos los claims, que son informaciones emitidas por una fuente confiable, pueden contener cualquier key/value que definamos y que son añadidas al TOKEN
            var claims = new List<Claim>()
            {
                new Claim("email",credencialesUsuario.Email) //Nunca enviar data sensible en un claim, ya que es leído por el cliente
            };

            //añadiendo claims de roles
            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);
            var claimsDB = await userManager.GetClaimsAsync(usuario);//obtengo los claims de la base de datos
            claims.AddRange(claimsDB);//añado los claims que el usuario tenga registrados a los claims que ya posee


            //firmando el JWT
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTKey"])); //nos valemos del proveedor de configuracion appsettings.Development.json para guardar una llaveJWT
            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);
            var expiracion = DateTime.UtcNow.AddDays(7);//se puede configurar cualquier espacio de tiempo de validez de un token según las reglas de negocio

            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, signingCredentials: credenciales, expires: expiracion);
            return new RespuestaAutenticacionDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };
        }

        [HttpPost("hacerAdmin", Name = "hacerAdmin")]
        public async Task<ActionResult> HacerAdmin(EditarAdminDTO editarAdmin)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdmin.Email);
            await userManager.AddClaimAsync(usuario, new Claim("isAdmin", "1"));

            return NoContent();
        }

        [HttpPost("removerAdmin", Name = "removerAdmin")]
        public async Task<ActionResult> RemoverAdmin(EditarAdminDTO editarAdmin)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdmin.Email);
            await userManager.RemoveClaimAsync(usuario, new Claim("isAdmin", "1"));

            return NoContent();
        }
    }
}
