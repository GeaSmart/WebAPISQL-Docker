using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.DTO
{
    public class HateoasDTO
    {
        public string Enlace { get; }
        public string Descripcion { get; }
        public string Metodo { get; }

        public HateoasDTO(string enlace, string descripcion, string metodo)
        {
            Enlace = enlace;
            Descripcion = descripcion;
            Metodo = metodo;
        }
    }
}
