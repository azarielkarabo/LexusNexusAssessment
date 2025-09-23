namespace LexusNexusAssessment.Models.Categories;
public record CreateCategoryDto(
    string Name,
    string? Description,
    int? ParentCategoryId
);