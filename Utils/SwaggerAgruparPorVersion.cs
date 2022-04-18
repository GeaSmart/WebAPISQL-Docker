using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryNetCoreAPI.Utils
{
    public class SwaggerAgruparPorVersion : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var namespaceControlador = controller.ControllerType.Namespace;//obtiene algo así como LibraryNetCoreAPI.Controllers.v1
            var versionApi = namespaceControlador.Split('.').Last().ToLower();
            controller.ApiExplorer.GroupName = versionApi;
        }
    }
}
