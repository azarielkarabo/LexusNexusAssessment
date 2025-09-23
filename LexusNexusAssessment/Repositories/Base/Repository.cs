using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace LexusNexusAssessment.Repositories.Base
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ConcurrentDictionary<int, T> _entities;
        private static int _nextId = 1;
        private static readonly object _idLock = new object();

        protected Repository()
        {
            _entities = new ConcurrentDictionary<int, T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            _entities.TryGetValue(id, out var entity);
            return await Task.FromResult(entity);
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync()
        {
            var entities = _entities.Values.ToList();
            if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
            {
                entities = entities.Cast<IComparable<T>>().OrderBy(x => x).Cast<T>().ToList();
            }
            return await Task.FromResult(entities.AsReadOnly());
        }

        public virtual async Task<IReadOnlyList<T>> GetPagedAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var entities = _entities.Values.AsQueryable()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return await Task.FromResult(entities.AsReadOnly());
        }

        public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            var compiled = predicate.Compile();
            var entities = _entities.Values.Where(compiled).ToList();
            return await Task.FromResult(entities.AsReadOnly());
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var id = GetNextId();
            SetEntityId(entity, id);
            SetTimestamps(entity, isNew: true);

            _entities.TryAdd(id, entity);
            return await Task.FromResult(entity);
        }

        public virtual async Task<T?> UpdateAsync(int id, T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (!_entities.TryGetValue(id, out var existingEntity))
                return null;

            UpdateEntityProperties(existingEntity, entity);
            SetTimestamps(existingEntity, isNew: false);

            return await Task.FromResult(existingEntity);
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            var success = _entities.TryRemove(id, out _);
            return await Task.FromResult(success);
        }

        public virtual async Task<int> CountAsync()
        {
            return await Task.FromResult(_entities.Count);
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            return await Task.FromResult(_entities.ContainsKey(id));
        }

        protected static int GetNextId()
        {
            lock (_idLock)
            {
                return _nextId++;
            }
        }

        private static void SetEntityId(T entity, int id)
        {
            var idProperty = typeof(T).GetProperty("Id");
            idProperty?.SetValue(entity, id);
        }

        private static void SetTimestamps(T entity, bool isNew)
        {
            var now = DateTime.UtcNow;

            if (isNew)
            {
                var createdAtProperty = typeof(T).GetProperty("CreatedAt");
                createdAtProperty?.SetValue(entity, now);
            }

            var updatedAtProperty = typeof(T).GetProperty("UpdatedAt");
            updatedAtProperty?.SetValue(entity, now);
        }

        protected virtual void UpdateEntityProperties(T existing, T updated)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.Name != "Id" && p.Name != "CreatedAt");

            foreach (var property in properties)
            {
                var value = property.GetValue(updated);
                if (value != null)
                {
                    property.SetValue(existing, value);
                }
            }
        }
    }

}
