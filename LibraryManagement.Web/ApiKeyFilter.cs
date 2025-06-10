namespace LibraryManagement.Services;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class ApiKeyFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if(operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-API-KEY",     
            In = ParameterLocation.Header,
            Required = true,
            Description = "API Key needed to access this endpoint"
        });
    }
}