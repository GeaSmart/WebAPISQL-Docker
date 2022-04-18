using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.DTO
{
    public class RespuestaAutenticacionDTO
    {
        public string Token { get; set; }
        public DateTime Expiracion { get; set; }
    }
}
