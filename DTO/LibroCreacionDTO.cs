using LibraryNetCoreAPI.Validaciones;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.DTO
{
    public class LibroCreacionDTO
    {
        [Required]
        [StringLength(maximumLength: 250)]
        [PrimeraLetraMayuscula]
        public string Titulo { get; set; }
        public DateTime FechaPublicacion { get; set; }

        public List<int> AutoresIds { get; set; }
    }
}
