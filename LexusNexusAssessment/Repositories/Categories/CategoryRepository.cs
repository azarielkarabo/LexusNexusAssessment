using LexusNexusAssessment.Models;
using LexusNexusAssessment.Repositories.Base;

namespace LexusNexusAssessment.Repositories.Categories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private static int _nextId = 1;
        private static readonly object _idLock = new object();

        public async Task<IReadOnlyList<Category>> GetCategoryTreeAsync()
        {
            var allCategories = _entities.Values.ToList();
            var rootCategories = allCategories.Where(c => !c.ParentCategoryId.HasValue).ToList();

            foreach (var category in allCategories)
            {
                category.SubCategories = allCategories
                    .Where(c => c.ParentCategoryId == category.Id)
                    .OrderBy(c => c.Name)
                    .ToList();
            }

            return await Task.FromResult(rootCategories.OrderBy(c => c.Name).ToList().AsReadOnly());
        }

        public override Task<Category> AddAsync(Category entity)
        {
            entity.Id = GetNextId();
            return base.AddAsync(entity);
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
