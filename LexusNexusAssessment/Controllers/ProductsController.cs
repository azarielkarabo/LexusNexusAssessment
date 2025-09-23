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
        public async Task<ActionResult> GetProducts([FromQuery] string? searchTerm = null, [FromQuery] int? categoryId = null,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortDirection = "asc")
        {
            var searchDto = new ProductSearchDto(searchTerm, categoryId, page, pageSize, sortBy, sortDirection);
            var products = await ExecuteSearch(searchDto);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return MapToDto(product);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createDto)
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

            var created = await _productRepository.AddAsync(product);
            return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, MapToDto(created));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(int id, UpdateProductDto updateDto)
        {
            var existing = await _productRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            // Apply updates from record
            if (updateDto.Name != null) existing.Name = updateDto.Name;
            if (updateDto.Description != null) existing.Description = updateDto.Description;
            if (updateDto.SKU != null) existing.SKU = updateDto.SKU;
            if (updateDto.Price.HasValue) existing.Price = updateDto.Price.Value;
            if (updateDto.Quantity.HasValue) existing.Quantity = updateDto.Quantity.Value;
            if (updateDto.CategoryId.HasValue) existing.CategoryId = updateDto.CategoryId;

            var updated = await _productRepository.UpdateAsync(id, existing);
            return Ok(MapToDto(updated!));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var success = await _productRepository.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }

        private async Task<object> ExecuteSearch(ProductSearchDto searchDto)
        {
            // Pattern matching on the search record
            var (products, totalCount) = searchDto switch
            {
                { SearchTerm: not null, CategoryId: not null } =>
                    await SearchWithTermAndCategory(searchDto),
                { SearchTerm: not null } =>
                    await SearchWithTerm(searchDto),
                _ => await GetPaginated(searchDto)
            };

            return new
            {
                products = products.Select(MapToDto),
                searchCriteria = searchDto,
                totalCount,
                page = searchDto.Page,
                pageSize = searchDto.PageSize
            };
        }

        private async Task<(IReadOnlyList<Product>, int)> SearchWithTermAndCategory(ProductSearchDto searchDto)
        {
            var all = await _productRepository.SearchAsync(searchDto.SearchTerm!, searchDto.CategoryId);
            var paged = all.Skip((searchDto.Page - 1) * searchDto.PageSize).Take(searchDto.PageSize).ToList();
            return (paged.AsReadOnly(), all.Count);
        }

        private async Task<(IReadOnlyList<Product>, int)> SearchWithTerm(ProductSearchDto searchDto)
        {
            var all = await _productRepository.SearchAsync(searchDto.SearchTerm!);
            var paged = all.Skip((searchDto.Page - 1) * searchDto.PageSize).Take(searchDto.PageSize).ToList();
            return (paged.AsReadOnly(), all.Count);
        }

        private async Task<(IReadOnlyList<Product>, int)> GetPaginated(ProductSearchDto searchDto)
        {
            var products = await _productRepository.GetPagedAsync(searchDto.Page, searchDto.PageSize);
            var total = await _productRepository.CountAsync();
            return (products, total);
        }

        private static ProductDto MapToDto(Product product) => new(
            product.Id, product.Name, product.Description, product.SKU,
            product.Price, product.Quantity, product.CategoryId,
            product.CreatedAt);
    }
}
