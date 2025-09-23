namespace LexusNexusAssessment.Models.Categories;
public record UpdateCategoryDto(
        string? Name,
        string? Description,
        int? ParentCategoryId
    );
