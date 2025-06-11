using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace LibraryManagement.Common;

public class AddCommonHeadersOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();

        // Add X-API-KEY header for all requests
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-API-KEY",
            In = ParameterLocation.Header,
            Required = true,
            Description = "API Key needed to access this endpoint",
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });

        // Add X-Idempotency-Key header only for POST requests
        var isPostMethod = context.ApiDescription.HttpMethod?.Equals("POST", StringComparison.OrdinalIgnoreCase) ?? false;
        if (isPostMethod)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Idempotency-Key",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Idempotency Key (GUID) to prevent duplicate requests",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Format = "uuid"
                }
            });
        }
    }
}
