using AutoMapper;
using LibraryNetCoreAPI.DTO;
using LibraryNetCoreAPI.Entidades;
using LibraryNetCoreAPI.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/libros/{libroId:int}/comentarios")]
    public class ComentariosController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUser> userManager;

        public ComentariosController(ApplicationDBContext context, IMapper mapper,UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        [HttpGet("{id:int}", Name = "obtenerComentario")]
        public async Task<ActionResult<ComentarioDTO>> GetById(int libroId, int id)
        {
            //el where asegura que el comentario corresponda al libro correcto
            var comentario = await context.Comentarios.Where(x => x.LibroId == libroId).FirstOrDefaultAsync(x => x.Id == id);

            if (comentario == null)
                return NotFound("Comentario no existe");

            return mapper.Map<ComentarioDTO>(comentario);
        }

        [HttpGet(Name = "obtenerComentariosPorLibro")]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId,[FromQuery] PaginacionDTO paginacionDTO)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro)
                return NotFound($"El libro con id {libroId} no existe");

            var queryable = context.Comentarios.Where(x => x.LibroId == libroId).AsQueryable();//movimos el where aquí porque el conteo de registros lo queremos con el filtro aplicado
            await HttpContext.InsertarPaginacionHeader(queryable);

            var comentarios = await queryable.OrderBy(x=>x.Id).Paginar(paginacionDTO).ToListAsync();
            return mapper.Map<List<ComentarioDTO>>(comentarios);
        }

        [HttpPost(Name = "crearComentario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var emailClaim = HttpContext.User.Claims.Where(x => x.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;

            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;

            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro)
                return NotFound($"El libro con id {libroId} no existe");

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.LibroId = libroId;
            comentario.UsuarioId = usuarioId;
            context.Comentarios.Add(comentario);
            await context.SaveChangesAsync();

            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);
            //return Ok(); ya no se devuelve un simple Ok
            return CreatedAtRoute("obtenerComentario", new { id = comentario.Id, libroId = libroId }, comentarioDTO);
        }

        [HttpPut("{id:int}", Name = "actualizarComentario")]
        public async Task<ActionResult> Put(int id, int libroId,  ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro)
                return NotFound($"El libro con id {libroId} no existe");

            var existeComentario = await context.Comentarios.AnyAsync(x => x.Id == id);
            if (!existeComentario)
                return NotFound($"El comentario con id {id} no existe");

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.Id = id;
            comentario.LibroId = libroId;

            context.Comentarios.Update(comentario);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "borrarComentario")]
        public async Task<ActionResult> Delete(int id, int libroId)
        {
            var existeComentario = await context.Comentarios.Where(x => x.LibroId == libroId).AnyAsync(x => x.Id == id);
            if (!existeComentario)
                return NotFound("El comentario no existe o no coincide con el libro especificado");

            context.Comentarios.Remove(new Comentario { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
