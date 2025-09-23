using LexusNexusAssessment.Models;
using LexusNexusAssessment.Repositories.Base;

namespace LexusNexusAssessment.Repositories;
public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByNameAsync(string productName);
    Task<IReadOnlyList<Product>> SearchAsync(string searchTerm, int? categoryId = null);
    Task<int> GetQuantityAsync(string productName);
}