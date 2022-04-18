using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.DTO
{
    public class ResultadoHashDTO
    {
        public string Hash { get; set; }
        public byte[] Sal { get; set; }
    }
}
