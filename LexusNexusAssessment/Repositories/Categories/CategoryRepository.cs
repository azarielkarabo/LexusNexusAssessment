using LexusNexusAssessment.Models;
using LexusNexusAssessment.Repositories.Base;
using Microsoft.Extensions.Caching.Memory;

namespace LexusNexusAssessment.Repositories.Categories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private static int _nextId = 1;
        private static readonly object _idLock = new object();

        public CategoryRepository(IMemoryCache memoryCache) : base(memoryCache)
        {

        }

        public IReadOnlyList<Category> GetCategoryTree()
        {
            var allCategories = _entities.Values.ToList();
            var rootCategories = allCategories.Where(c => !c.ParentCategoryId.HasValue).ToList();

            foreach (var category in allCategories)
            {
                category.ChildCategories = allCategories
                    .Where(c => c.ParentCategoryId == category.Id)
                    .OrderBy(c => c.Name)
                    .ToList();
            }

            return rootCategories.OrderBy(c => c.Name).ToList().AsReadOnly();
        }

        public override Category Add(Category entity)
        {
            entity.Id = GetNextId();
            return base.Add(entity);
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
