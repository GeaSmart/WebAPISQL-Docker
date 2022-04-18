using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.Entidades
{
    public class Autor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(maximumLength:120, ErrorMessage = "El campo {0} no debe tener más de {1} caracteres.")]
        public string Nombre { get; set; }

        //propiedad de navegación
        public List<AutorLibro> AutoresLibros { get; set; }
    }
}
