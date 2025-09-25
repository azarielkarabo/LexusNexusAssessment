namespace LexusNexusAssessment.Models.Products
{
    public record ProductSearchResultDto(
          IEnumerable<ProductDto> Products,
          ProductSearchDto SearchCriteria,
          int TotalCount,
          int Page,
          int PageSize
      );
}
