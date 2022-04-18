using AutoMapper;
using LibraryNetCoreAPI.DTO;
using LibraryNetCoreAPI.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.Utils
{
    public class AutoMapperProfiles:Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AutorCreacionDTO, Autor>();
            CreateMap<Autor, AutorDTO>();
            CreateMap<Autor, AutorConLibrosDTO>()
                .ForMember(x => x.Libros, options => options.MapFrom(MapFromAutoresLibrosToLibrosDTO));
            CreateMap<Libro, LibroDTO>();
            CreateMap<Libro, LibroConAutoresDTO>()
                .ForMember(x => x.Autores, options => options.MapFrom(MapFromAutoresLibrosToAutoresDTO));
            CreateMap<LibroCreacionDTO, Libro>()
                .ForMember(x => x.AutoresLibros, options => options.MapFrom(MapAutoresLibros));
            CreateMap<LibroPatchDTO, Libro>().ReverseMap();
            CreateMap<ComentarioCreacionDTO, Comentario>();
            CreateMap<Comentario, ComentarioDTO>();
        }

        private List<LibroDTO> MapFromAutoresLibrosToLibrosDTO(Autor autor, AutorDTO autorDTO)
        {
            List<LibroDTO> response = new List<LibroDTO>();
            if (autor.AutoresLibros == null)
                return response;

            foreach(var item in autor.AutoresLibros)            
                response.Add(new LibroDTO { Id = item.LibroId, Titulo = item.Libro.Titulo });
            
            return response;
        }

        private List<AutorDTO> MapFromAutoresLibrosToAutoresDTO(Libro libro, LibroDTO libroDTO)
        {
            List<AutorDTO> response = new List<AutorDTO>();
            if (libro.AutoresLibros == null)
                return response;

            foreach(var item in libro.AutoresLibros)
            {
                response.Add(new AutorDTO { Id = item.Autor.Id, Nombre = item.Autor.Nombre });
            }
            return response;
        }

        private List<AutorLibro> MapAutoresLibros(LibroCreacionDTO libroCreacionDTO, Libro libro)
        {
            List<AutorLibro> response = new List<AutorLibro>();

            if (libroCreacionDTO.AutoresIds == null)
                return response;
            
            foreach(var item in libroCreacionDTO.AutoresIds)
            {
                response.Add(new AutorLibro { AutorId = item });
            }

            return response;
        }
    }
}
