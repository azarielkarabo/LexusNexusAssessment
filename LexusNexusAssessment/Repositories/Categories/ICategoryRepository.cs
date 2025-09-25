using LexusNexusAssessment.Models;
using LexusNexusAssessment.Models.Categories;
using LexusNexusAssessment.Repositories.Base;

namespace LexusNexusAssessment.Repositories.Categories
{
    public interface ICategoryRepository: IRepository<Category>
    {
        IReadOnlyList<Category> GetCategoryTree();
    }
}
