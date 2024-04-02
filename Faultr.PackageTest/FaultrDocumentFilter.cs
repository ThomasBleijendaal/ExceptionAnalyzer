using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Faultr.PackageTest;

internal class FaultrDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var method = Exceptions.Current.Methods[0];
        var responses = swaggerDoc.Paths["/"].Operations[OperationType.Get].Responses;

        var i = 0;
        foreach (var exception in method.ThrownExceptions)
        {
            i++;

            responses[$"500-{i}"] = new OpenApiResponse
            {
                Description = exception.Exception?.Message ?? "Internal Server Error"
            };
        }
    }
}
