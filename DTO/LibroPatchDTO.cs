using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.DTO
{
    public class LibroPatchDTO
    {
        [Required]
        [StringLength(maximumLength: 250)]
        public string Titulo { get; set; }
        public DateTime FechaPublicacion { get; set; }
    }
}
