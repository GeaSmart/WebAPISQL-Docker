using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.DTO
{
    public class RecursoDTO
    {
        public List<HateoasDTO> Enlaces { get; set; } = new List<HateoasDTO>();
    }
}
