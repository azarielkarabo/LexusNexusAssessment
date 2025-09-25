using LexusNexusAssessment.Models;
using LexusNexusAssessment.Models.Products;
using LexusNexusAssessment.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LexusNexusAssessment.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet]
        public ActionResult<ProductSearchResultDto> GetProducts([FromQuery] string? searchTerm = null, [FromQuery] int? categoryId = null,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortDirection = "asc")
        {
            var searchDto = new ProductSearchDto(searchTerm, categoryId, page, pageSize, sortBy, sortDirection);
            var products = ExecuteSearch(searchDto);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public ActionResult<ProductDto> GetProduct(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
                return NotFound();

            return MapToDto(product);
        }

        [HttpPost]
        public ActionResult<ProductDto> CreateProduct(CreateProductDto createDto)
        {
            var product = new Product
            {
                Name = createDto.Name,
                Description = createDto.Description,
                SKU = createDto.SKU,
                Price = createDto.Price,
                Quantity = createDto.Quantity,
                CategoryId = createDto.CategoryId
            };

            var created = _productRepository.Add(product);
            return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, MapToDto(created));
        }

        [HttpPut("{id}")]
        public ActionResult<ProductDto> UpdateProduct(int id, UpdateProductDto updateDto)
        {
            var existing = _productRepository.GetById(id);
            if (existing == null)
                return NotFound();

            // Apply updates from record
            if (updateDto.Name != null) existing.Name = updateDto.Name;
            if (updateDto.Description != null) existing.Description = updateDto.Description;
            if (updateDto.SKU != null) existing.SKU = updateDto.SKU;
            if (updateDto.Price.HasValue) existing.Price = updateDto.Price.Value;
            if (updateDto.Quantity.HasValue) existing.Quantity = updateDto.Quantity.Value;
            if (updateDto.CategoryId.HasValue) existing.CategoryId = updateDto.CategoryId;

            var updated = _productRepository.Update(id, existing);
            return Ok(MapToDto(updated!));
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteProduct(int id)
        {
            var success = _productRepository.Delete(id);
            return success ? NoContent() : NotFound();
        }

        private ProductSearchResultDto ExecuteSearch(ProductSearchDto searchDto)
        {
            // Pattern matching on the search record
            var (products, totalCount) = searchDto switch
            {
                { SearchTerm: not null, CategoryId: not null } =>
                     SearchWithTermAndCategory(searchDto),
                { SearchTerm: not null } =>
                     SearchWithTerm(searchDto),
                _ => GetPaginated(searchDto)
            };

            return new ProductSearchResultDto(
                 products.Select(MapToDto),
                 searchDto,
                 totalCount,
                 searchDto.Page,
                 searchDto.PageSize
            );
        }

        private (IReadOnlyList<Product>, int) SearchWithTermAndCategory(ProductSearchDto searchDto)
        {
            var all = _productRepository.Search(searchDto.SearchTerm!, searchDto.CategoryId);
            var paged = all.Skip((searchDto.Page - 1) * searchDto.PageSize).Take(searchDto.PageSize).ToList();
            return (paged.AsReadOnly(), all.Count);
        }

        private (IReadOnlyList<Product>, int) SearchWithTerm(ProductSearchDto searchDto)
        {
            var all = _productRepository.Search(searchDto.SearchTerm!);
            var paged = all.Skip((searchDto.Page - 1) * searchDto.PageSize).Take(searchDto.PageSize).ToList();
            return (paged.AsReadOnly(), all.Count);
        }

        private (IReadOnlyList<Product>, int) GetPaginated(ProductSearchDto searchDto)
        {
            var products = _productRepository.GetPaged(searchDto.Page, searchDto.PageSize);
            var total = _productRepository.Count();
            return (products, total);
        }

        private static ProductDto MapToDto(Product product) => new(
            product.Id, product.Name, product.Description, product.SKU,
            product.Price, product.Quantity, product.CategoryId,
            product.CreatedAt);
    }
}
