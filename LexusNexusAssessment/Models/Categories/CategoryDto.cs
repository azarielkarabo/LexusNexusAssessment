namespace LexusNexusAssessment.Models.Categories;
public record CategoryDto(
       int Id,
       string Name,
       string? Description,
       int? ParentCategoryId,
       List<CategoryDto> ChildCategories
   );