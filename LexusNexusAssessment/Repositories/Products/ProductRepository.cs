using LexusNexusAssessment.Models;
using LexusNexusAssessment.Repositories.Base;
using System.Collections.Concurrent;

namespace LexusNexusAssessment.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private static int _nextId = 1;
        private static readonly object _idLock = new object();

        // Override base methods to use the static dictionary
        public override async Task<Product?> GetByIdAsync(int id)
        {
            _entities.TryGetValue(id, out var product);
            return await Task.FromResult(product);
        }

        public override async Task<IReadOnlyList<Product>> GetAllAsync()
        {
            var products = _entities.Values.OrderBy(p => p.Name).ToList();
            return await Task.FromResult(products.AsReadOnly());
        }

        public override async Task<IReadOnlyList<Product>> GetPagedAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var products = _entities.Values
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return await Task.FromResult(products.AsReadOnly());
        }

        public override async Task<Product> AddAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (string.IsNullOrWhiteSpace(product.Name))
                throw new ArgumentException("Product name cannot be null or empty", nameof(product));

            if (product.Quantity < 0)
                throw new ArgumentException("Quantity cannot be negative", nameof(product));

            // Check for existing product by name
            var existingProduct = _entities.Values
                .FirstOrDefault(p => string.Equals(p.Name, product.Name, StringComparison.OrdinalIgnoreCase));

            if (existingProduct != null)
            {
                existingProduct.Quantity += product.Quantity;
                if (product.Price > 0) existingProduct.Price = product.Price;
                if (!string.IsNullOrEmpty(product.Description)) existingProduct.Description = product.Description;
                if (!string.IsNullOrEmpty(product.SKU)) existingProduct.SKU = product.SKU;
                if (product.CategoryId.HasValue) existingProduct.CategoryId = product.CategoryId;

                return existingProduct;
            }

            product.Id = GetNextId();
            product.CreatedAt = DateTime.UtcNow;

            _entities.TryAdd(product.Id, product);
            return product;
        }

        public override async Task<Product?> UpdateAsync(int id, Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (!_entities.TryGetValue(id, out var existingProduct))
                return null;

            existingProduct.Name = product.Name ?? existingProduct.Name;
            existingProduct.Description = product.Description ?? existingProduct.Description;
            existingProduct.SKU = product.SKU ?? existingProduct.SKU;
            existingProduct.Price = product.Price;
            existingProduct.Quantity = product.Quantity;
            existingProduct.CategoryId = product.CategoryId;

            return await Task.FromResult(existingProduct);
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            var success = _entities.TryRemove(id, out _);
            return await Task.FromResult(success);
        }

        public override async Task<int> CountAsync()
        {
            return await Task.FromResult(_entities.Count);
        }

        // Additional ProductRepository-specific methods
        public async Task<Product?> GetByNameAsync(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Product name cannot be null or empty", nameof(productName));

            var product = _entities.Values
                .FirstOrDefault(p => string.Equals(p.Name, productName, StringComparison.OrdinalIgnoreCase));

            return await Task.FromResult(product);
        }

        public async Task<IReadOnlyList<Product>> SearchAsync(string searchTerm, int? categoryId = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var query = _entities.Values.AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            var results = query
                .Where(p =>
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (p.Description != null && p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (p.SKU != null && p.SKU.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    IsLikelyMatch(p.Name, searchTerm)
                )
                .OrderByDescending(p => GetRelevanceScore(p, searchTerm))
                .ToList();

            return await Task.FromResult(results.AsReadOnly());
        }

        public async Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId)
        {
            var products = _entities.Values
                .Where(p => p.CategoryId == categoryId)
                .OrderBy(p => p.Name)
                .ToList();

            return await Task.FromResult(products.AsReadOnly());
        }

        public async Task<int> GetQuantityAsync(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Product name cannot be null or empty", nameof(productName));

            var product = _entities.Values
                .FirstOrDefault(p => string.Equals(p.Name, productName, StringComparison.OrdinalIgnoreCase));

            return await Task.FromResult(product?.Quantity ?? 0);
        }

        private static bool IsLikelyMatch(string productName, string searchTerm)
        {
            if (Math.Abs(productName.Length - searchTerm.Length) > 2)
                return false;

            var productLower = productName.ToLower();
            var searchLower = searchTerm.ToLower();

            int differences = 0;
            int minLength = Math.Min(productLower.Length, searchLower.Length);

            for (int i = 0; i < minLength; i++)
            {
                if (productLower[i] != searchLower[i])
                    differences++;

                if (differences > 2)
                    return false;
            }

            return differences <= 2;
        }

        private static double GetRelevanceScore(Product product, string searchTerm)
        {
            double score = 0;

            if (product.Name.Equals(searchTerm, StringComparison.OrdinalIgnoreCase))
                score += 100;

            if (product.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                score += 50;

            if (product.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                score += 25;

            if (product.SKU?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                score += 30;

            return score;
        }

        private static int GetNextId()
        {
            lock (_idLock)
            {
                return _nextId++;
            }
        }
    }
}