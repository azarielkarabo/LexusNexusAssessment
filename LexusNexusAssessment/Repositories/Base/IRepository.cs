using System.Linq.Expressions;

namespace LexusNexusAssessment.Repositories.Base;

public interface IRepository<T> where T : class, IEntity
{
    // Synchronous methods
    T? GetById(int id);
    IReadOnlyList<T> GetAll();
    IReadOnlyList<T> GetPaged(int page, int pageSize);
    IReadOnlyList<T> Find(Expression<Func<T, bool>> predicate);
    T? FindFirst(Expression<Func<T, bool>> predicate);
    T Add(T entity);
    IEnumerable<T> AddRange(IEnumerable<T> entities);
    T? Update(int id, T entity);
    T? Update(T entity);
    bool Delete(int id);
    int DeleteRange(Expression<Func<T, bool>> predicate);
    int Count();
    int Count(Expression<Func<T, bool>> predicate);
    bool Exists(int id);
    bool Any(Expression<Func<T, bool>> predicate);
    void Clear();
}
