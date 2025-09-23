using System.Linq.Expressions;

namespace LexusNexusAssessment.Repositories.Base;
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<IReadOnlyList<T>> GetPagedAsync(int page, int pageSize);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<T?> UpdateAsync(int id, T entity);
    Task<bool> DeleteAsync(int id);
    Task<int> CountAsync();
    Task<bool> ExistsAsync(int id);
}