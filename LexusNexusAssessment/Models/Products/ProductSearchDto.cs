namespace LexusNexusAssessment.Models.Products;
public record ProductSearchDto(
      string? SearchTerm,
      int? CategoryId,
      int Page = 1,
      int PageSize = 10,
      string? SortBy = "name",
      string? SortDirection = "asc"
  );