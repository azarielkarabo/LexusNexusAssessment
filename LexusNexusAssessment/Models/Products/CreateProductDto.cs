namespace LexusNexusAssessment.Models.Products;
public record CreateProductDto(
    string Name,
    string? Description,
    string? SKU,
    decimal Price,
    int Quantity,
    int? CategoryId
);