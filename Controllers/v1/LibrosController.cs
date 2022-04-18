using AutoMapper;
using LibraryNetCoreAPI.DTO;
using LibraryNetCoreAPI.Entidades;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IMapper mapper;

        public LibrosController(ApplicationDBContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet("{id:int}", Name = "obtenerLibro")]
        public async Task<ActionResult<LibroConAutoresDTO>> Get(int id)
        {
            var libro = await context.Libros
                .Include(x => x.AutoresLibros)
                .ThenInclude(x => x.Autor)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libro == null)
                return NotFound("El libro no existe");

            libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList(); //ordenando la lista de autores por el campo orden
            return mapper.Map<LibroConAutoresDTO>(libro);
        }

        [HttpPost(Name = "crearLibro")]        
        public async Task<ActionResult> Post([FromBody] LibroCreacionDTO libroCreacionDTO)
        {
            if (libroCreacionDTO.AutoresIds == null)
                return BadRequest("No se puede crear un libro sin autor");

            var autoresIds = await context.Autores.Where(x => libroCreacionDTO.AutoresIds.Contains(x.Id))
                .Select(x => x.Id).ToListAsync();

            if (autoresIds.Count != libroCreacionDTO.AutoresIds.Count)
                return BadRequest("Se ingreso al menos un autor no registrado");

            var libro = mapper.Map<Libro>(libroCreacionDTO);

            AsignarOrdenAutores(libro);

            context.Libros.Add(libro);
            await context.SaveChangesAsync();

            var libroDTO = mapper.Map<LibroDTO>(libro);
            //return Ok();
            return CreatedAtRoute("obtenerLibro", new { id = libro.Id }, libroDTO);
        }

        [HttpPut("{id:int}", Name = "actualizarLibro")]
        public async Task<ActionResult> Put(int id, LibroCreacionDTO libroCreacionDTO)
        {
            var libro = await context.Libros.Include(x => x.AutoresLibros).FirstOrDefaultAsync(x => x.Id == id);

            if(libro == null)
                return NotFound("El libro no existe");

            libro = mapper.Map(libroCreacionDTO, libro); //mapeando objetos existentes de origen y destino

            AsignarOrdenAutores(libro);
                        
            context.Libros.Update(libro);
            await context.SaveChangesAsync();
            return NoContent();
        }

        private void AsignarOrdenAutores(Libro libro)
        {
            for (int i = 0; i < libro.AutoresLibros.Count; i++)
            {
                libro.AutoresLibros[i].Orden = i;
            }
        }

        [HttpPatch("{id:int}", Name = "patchLibro")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<LibroPatchDTO> jsonPatchDocument)
        {
            if (jsonPatchDocument == null)
                return BadRequest();

            var libro = await context.Libros.FirstOrDefaultAsync(x => x.Id == id);

            if (libro == null)
                return NotFound("Libro no existe");

            var libroPatchDTO = mapper.Map<LibroPatchDTO>(libro); //lleno el jsonPatchDocument con el libro traido de la bd

            jsonPatchDocument.ApplyTo(libroPatchDTO, ModelState); //le aplico los cambios que vinieron el el jsonPatchDocument

            if (!TryValidateModel(libroPatchDTO))
                return BadRequest(ModelState);

            mapper.Map(libroPatchDTO,libro);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "borrarLibro")]
        public async Task<ActionResult> Delete(int id)
        {
            var existeAutor = await context.Libros.AnyAsync(x => x.Id == id);
            if (!existeAutor)
                return NotFound("El autor no existe");

            context.Libros.Remove(new Libro { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
