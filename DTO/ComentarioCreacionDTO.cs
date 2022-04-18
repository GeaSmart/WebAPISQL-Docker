using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.DTO
{
    public class ComentarioCreacionDTO
    {
        [Required]
        public string Contenido { get; set; }
    }
}
