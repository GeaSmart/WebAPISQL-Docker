using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.DTO
{
    public class AutorDTO : RecursoDTO
    {        
        public int Id { get; set; }
        public string Nombre { get; set; }
        
    }
}
