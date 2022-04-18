using Microsoft.AspNetCore.Mvc.ActionConstraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.Utils
{
    public class VersionHeaderAttribute : Attribute, IActionConstraint // IActionConstraint nos permite tener una lógica personalizada para definir qué endpoint yo quiero que se ejecute
    {
        private readonly string cabecera;
        private readonly string valor;

        public VersionHeaderAttribute(string cabecera, string valor)
        {
            this.cabecera = cabecera;
            this.valor = valor;
        }
        public int Order => 0;

        public bool Accept(ActionConstraintContext context)
        {
            //aquí va la lógica que le dice al api si la petición http tiene la cabecera y valor indicado va a utilizar el endpoint donde se encuentra este atributo
            var cabeceras = context.RouteContext.HttpContext.Request.Headers;

            var method = context.RouteContext.HttpContext.Request.Method;

            if (method != "GET")
                return true;

            if (!cabeceras.ContainsKey(cabecera))
                return false;

            return string.Equals(cabeceras[cabecera], valor, StringComparison.OrdinalIgnoreCase);
        }
    }
}
