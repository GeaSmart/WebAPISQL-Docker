using AutoMapper;
using LibraryNetCoreAPI.DTO;
using LibraryNetCoreAPI.Entidades;
using LibraryNetCoreAPI.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.Controllers.v1
{
    [ApiController]
    [Route("api/[controller]")]
    //[Route("api/v1/[controller]")]
    [VersionHeader("version","1")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "isAdmin")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class AutoresController:ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;

        public AutoresController(ApplicationDBContext context, IMapper mapper, IConfiguration configuration)
        {
            this.context = context;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        [HttpGet("AppConfiguration")]
        public ActionResult<List<string>> GetConfiguration()
        {
            var lista = new List<string>();
            lista.Add(configuration["app-author"]);
            lista.Add(configuration["connectionStrings:defaultConnection"]);
            lista.Add(configuration.GetConnectionString("defaultConnection"));
            lista.Add(configuration["user-secret-1"]);
            lista.Add(configuration["test"]);

            return lista;
        }

        /// <summary>
        /// Obtiene todos los autores-Contenido añadido para probar pipelines en la versión 1 del api
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "obtenerAutoresv1")]
        [AllowAnonymous]  
        public async Task<List<AutorDTO>> Get([FromQuery]PaginacionDTO paginacionDTO)
        {
            var queryable = context.Autores.AsQueryable();
            await HttpContext.InsertarPaginacionHeader(queryable);//aquí estamos enviando al cliente la cantidad de registros en la cabecera
            //var autores = await context.Autores.ToListAsync();
            var autores = await queryable.OrderBy(x => x.Nombre).Paginar(paginacionDTO).ToListAsync();//aqui paginamos, se recomienda ordenar por algún campo, en este caso el nombre
            return mapper.Map<List<AutorDTO>>(autores);
        }

        /// <summary>
        /// Obtiene un autor
        /// </summary>
        /// <param name="id">Id del autor</param>
        /// <returns></returns>
        [HttpGet("{id:int}", Name = "obtenerAutorv1")]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AutorConLibrosDTO>> Get(int id)
        {
            var autor = await context.Autores
                .Include(x=>x.AutoresLibros)
                .ThenInclude(x=>x.Libro)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (autor == null)
                return NotFound($"El autor con id {id} no existe.");

            var dto =  mapper.Map<AutorConLibrosDTO>(autor);
            GenerarEnlaces(dto);
            return dto;
        }

        private void GenerarEnlaces(AutorDTO autorDTO)
        {
            autorDTO.Enlaces.Add(new HateoasDTO(
                enlace:Url.Link("obtenerAutor", new { id = autorDTO.Id }),
                descripcion:"self",
                metodo:"GET"
                ));
            autorDTO.Enlaces.Add(new HateoasDTO(
                enlace: Url.Link("actualizarAutor", new { id = autorDTO.Id }),
                descripcion: "autor-actualizar",
                metodo: "PUT"
                ));
            autorDTO.Enlaces.Add(new HateoasDTO(
                enlace: Url.Link("borrarAutor", new { id = autorDTO.Id }),
                descripcion: "autor-borrar",
                metodo: "DELETE"
                ));
        }

        [HttpGet("{nombre}", Name = "obtenerAutorPorNombrev1")]
        public async Task<ActionResult<List<AutorDTO>>> Get([FromRoute] string nombre)
        {
            var autores = await context.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();
            return mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpPost(Name = "crearAutor")]
        public async Task<ActionResult> Post([FromBody] AutorCreacionDTO autorCreacionDTO)
        {
            var existe = await context.Autores.AnyAsync(x => x.Nombre == autorCreacionDTO.Nombre);
            if (existe)
                return BadRequest($"Ya existe un autor con el nombre {autorCreacionDTO.Nombre}");

            //context.Add(autor); antes le pasábamos una instancia de Autor directamente, ahora mapeamos autorCreacionDTO a Autor y le enviamos
            var autor = mapper.Map<Autor>(autorCreacionDTO);
            context.Add(autor);

            await context.SaveChangesAsync();

            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("obtenerAutor", new { id = autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}", Name = "actualizarAutorv1")]
        public async Task<ActionResult> Put(int id, AutorCreacionDTO autorCreacionDTO)
        {
            var existeAutor = await context.Autores.AnyAsync(x=>x.Id == id);
            if (!existeAutor)
                return NotFound("No existe el autor");

            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;

            context.Update(autor);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "borrarAutorv1")]
        public async Task<ActionResult> Delete(int id)
        {
            var existeAutor = await context.Autores.AnyAsync(x => x.Id == id);
            if (!existeAutor)
                return NotFound("El autor no existe");

            context.Autores.Remove(new Autor { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
