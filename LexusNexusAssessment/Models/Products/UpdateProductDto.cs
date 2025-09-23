namespace LexusNexusAssessment.Models.Products;
public record UpdateProductDto(
    string? Name,
    string? Description,
    string? SKU,
    decimal? Price,
    int? Quantity,
    int? CategoryId
);

