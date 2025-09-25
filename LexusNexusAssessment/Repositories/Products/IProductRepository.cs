using LexusNexusAssessment.Models;
using LexusNexusAssessment.Repositories.Base;

namespace LexusNexusAssessment.Repositories;
public interface IProductRepository : IRepository<Product>
{
    Product? GetByName(string productName);
    IReadOnlyList<Product> Search(string searchTerm, int? categoryId = null);
    int GetQuantity(string productName);
}