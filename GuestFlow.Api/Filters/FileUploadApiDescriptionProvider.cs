using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace GuestFlow.Api.Filters
{
    /// <summary>
    /// IFormFile parametrelerini API description'dan kaldırmak için provider
    /// Bu, Swashbuckle'ın parametre okuma aşamasındaki hatayı önler
    /// </summary>
    public class FileUploadApiDescriptionProvider : IApiDescriptionProvider
    {
        public int Order => -1000; // Çok erken çalışsın (diğer provider'lardan önce)

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            // Parametreleri daha erken kaldırmak için burada işlem yapabiliriz
            // Ama genellikle OnProvidersExecuted'da yapılır
        }

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
            foreach (var apiDescription in context.Results)
            {
                if (apiDescription.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
                {
                    // IFormFile parametrelerini kaldır
                    // BindingSource.Form ile IFormFile tipini kontrol et
                    var formFileParams = new List<ApiParameterDescription>();
                    
                    foreach (var param in apiDescription.ParameterDescriptions)
                    {
                        if (IsFormFileType(param.Type) || 
                            (param.Source == BindingSource.Form && IsFormFileType(param.Type)))
                        {
                            formFileParams.Add(param);
                        }
                    }

                    // Parametreleri kaldır
                    foreach (var param in formFileParams)
                    {
                        apiDescription.ParameterDescriptions.Remove(param);
                    }
                }
            }
        }

        private bool IsFormFileType(System.Type? type)
        {
            if (type == null)
                return false;

            if (type == typeof(IFormFile))
                return true;

            if (type == typeof(IFormFile[]))
                return true;

            if (type.IsGenericType && 
                type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>) &&
                type.GetGenericArguments().Length > 0 &&
                type.GetGenericArguments()[0] == typeof(IFormFile))
                return true;

            return false;
        }
    }
}

