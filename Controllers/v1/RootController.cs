using LibraryNetCoreAPI.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.Controllers.v1
{
    [ApiController]
    [Route("api/v1")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RootController:ControllerBase
    {
        private readonly IAuthorizationService authorizationService;

        public RootController(IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
        }

        [HttpGet(Name = "obtenerRoot")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<HateoasDTO>>> Get()
        {
            var datosHateoas = new List<HateoasDTO>();
            var esAdmin = await authorizationService.AuthorizeAsync(User, "isAdmin");

            datosHateoas.Add(new HateoasDTO(enlace:Url.Link("obtenerRoot", new { }), descripcion: "self", metodo: "GET"));
            datosHateoas.Add(new HateoasDTO(enlace: Url.Link("obtenerAutores", new { }), descripcion: "autores", metodo: "GET"));            
            datosHateoas.Add(new HateoasDTO(enlace: Url.Link("crearLibro", new { }), descripcion: "libro-crear", metodo: "POST"));

            if (esAdmin.Succeeded)
            {
                datosHateoas.Add(new HateoasDTO(enlace: Url.Link("crearAutor", new { }), descripcion: "autor-crear", metodo: "POST"));
            }

            return datosHateoas;
        }
    }
}
