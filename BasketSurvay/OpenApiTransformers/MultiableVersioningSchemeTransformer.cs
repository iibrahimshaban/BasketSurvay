using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace BasketSurvay.OpenApiTransformers;

public sealed class MultiableVersioningSchemeTransformer(ApiVersionDescription description) : IOpenApiDocumentTransformer
{
    private readonly ApiVersionDescription description = description;

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new()
        {
            Title = "Survey Basket API",
            Version = description.ApiVersion.ToString(),
            Description = $"API_Description.{(description.IsDeprecated ? "This API version has been deprecated." : string.Empty)}"
        };

        return Task.CompletedTask;
    }
}
