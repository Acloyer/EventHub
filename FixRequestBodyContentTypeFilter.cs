using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EventHub.Filters
{
    public class FixRequestBodyContentTypeFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.RequestBody?.Content.ContainsKey("application/json") == true)
            {
                var mediaType = operation.RequestBody.Content["application/json"];
                // Добавим второй вариант с charset
                operation.RequestBody.Content["application/json; charset=utf-8"] = mediaType;
            }
        }
    }
}
