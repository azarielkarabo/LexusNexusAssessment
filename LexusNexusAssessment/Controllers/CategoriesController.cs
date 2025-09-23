using LexusNexusAssessment.Models;
using LexusNexusAssessment.Models.Categories;
using LexusNexusAssessment.Repositories.Categories;
using Microsoft.AspNetCore.Mvc;

namespace LexusNexusAssessment.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet("")]
        public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAllCategoriesTree()
        {
            var categoryTree = await _categoryRepository.GetAllAsync();
            return Ok(categoryTree);
        }

        [HttpGet("tree")]
        public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetCategoriesTree()
        {
            var categoryTree = await _categoryRepository.GetCategoryTreeAsync();
            return Ok(categoryTree);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createDto)
        {
            var validation = ValidateCreateCategory(createDto);
            if (!validation.IsValid)
                return BadRequest(validation.ErrorMessage);

            var created = await _categoryRepository.AddAsync(new Category
            {
                Name = createDto.Name,
                ParentCategoryId = createDto.ParentCategoryId
            });
            return Ok(created);
        }

        private ValidationResult ValidateCreateCategory(CreateCategoryDto dto)
        {
            return dto switch
            {
                { Name: null or "" } => ValidationResult.Failure("Category name is required"),
                { Name: var name } when name.Length > 100 => ValidationResult.Failure("Name too long"),
                _ => ValidationResult.Success()
            };
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public string? ErrorMessage { get; private set; }
        private ValidationResult(bool isValid, string? errorMessage = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }
        public static ValidationResult Success() => new(true);
        public static ValidationResult Failure(string errorMessage) => new(false, errorMessage);
    }
}
