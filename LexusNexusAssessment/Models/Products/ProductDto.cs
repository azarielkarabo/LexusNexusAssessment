namespace LexusNexusAssessment.Models.Products;
public record ProductDto(
     int Id,
     string Name,
     string? Description,
     string? SKU,
     decimal Price,
     int Quantity,
     int? CategoryId,
     DateTime CreatedAt
 );