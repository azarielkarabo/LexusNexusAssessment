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
        public ActionResult<IReadOnlyList<CategoryDto>> GetAllCategoriesTree()
        {
            var categoryTree = _categoryRepository.GetAll();
            return Ok(categoryTree);
        }

        [HttpGet("tree")]
        public ActionResult<IReadOnlyList<CategoryDto>> GetCategoriesTree()
        {
            var categoryTree = _categoryRepository.GetCategoryTree();
            return Ok(categoryTree);
        }

        [HttpPost]
        public ActionResult<CategoryDto> CreateCategory(CreateCategoryDto createDto)
        {
            var validation = ValidateCreateCategory(createDto);
            if (!validation.IsValid)
                return BadRequest(validation.ErrorMessage);

            var created = _categoryRepository.Add(new Category
            {
                Name = createDto.Name,
                Description = createDto.Description,
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
