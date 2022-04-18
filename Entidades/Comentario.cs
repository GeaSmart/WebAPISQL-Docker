using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.Entidades
{
    public class Comentario
    {
        public int Id { get; set; }

        [Required]
        public string Contenido { get; set; }
        public int LibroId { get; set; }
        public string UsuarioId { get; set; }

        //Propiedad de navegación
        public Libro Libro { get; set; }
        public IdentityUser Usuario { get; set; }
    }
}
