using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.Entidades
{
    public class Libro
    {
        public int Id { get; set; }

        [Required]
        [StringLength(maximumLength: 250)]        
        public string Titulo { get; set; }

        public DateTime FechaPublicacion { get; set; }

        //propiedad de navegación
        public List<Comentario> Comentarios { get; set; }                
        public List<AutorLibro> AutoresLibros { get; set; }
    }
}
