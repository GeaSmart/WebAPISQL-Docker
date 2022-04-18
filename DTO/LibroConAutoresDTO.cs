using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.DTO
{
    public class LibroConAutoresDTO : LibroDTO
    {
        public List<AutorDTO> Autores { get; set; }
    }
}
