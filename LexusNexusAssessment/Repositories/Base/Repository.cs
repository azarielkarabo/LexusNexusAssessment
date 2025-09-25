using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;

namespace LexusNexusAssessment.Repositories.Base
{
    public interface IEntity
    {
        int Id { get; set; }
        DateTime CreatedAt { get; set; }
    }

    public class Repository<T> : IRepository<T> where T : class, IEntity
    {
        private readonly IMemoryCache _cache;
        private readonly string _cacheKey;
        protected readonly ConcurrentDictionary<int, T> _entities;

        private static int _nextId = 1;
        private static readonly object _idLock = new object();

        public Repository(IMemoryCache cache)
        {
            _cache = cache;
            _cacheKey = $"Repository_{typeof(T).Name}";

            if (!_cache.TryGetValue(_cacheKey, out _entities!))
            {
                _entities = new ConcurrentDictionary<int, T>();

                _cache.Set(_cacheKey, _entities, new MemoryCacheEntryOptions
                {
                    Priority = CacheItemPriority.High
                });
            }
        }

        public virtual T? GetById(int id)
        {
            _entities.TryGetValue(id, out var entity);
            return entity;
        }

        public virtual IReadOnlyList<T> GetAll()
        {
            return _entities.Values
                .OrderBy(x => x.Id)
                .ToList()
                .AsReadOnly();
        }

        public virtual IReadOnlyList<T> GetPaged(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            return _entities.Values
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .AsReadOnly();
        }

        public virtual IReadOnlyList<T> Find(Expression<Func<T, bool>> predicate)
        {
            var compiled = predicate.Compile();
            return _entities.Values
                .Where(compiled)
                .OrderBy(x => x.Id)
                .ToList()
                .AsReadOnly();
        }

        public virtual T? FindFirst(Expression<Func<T, bool>> predicate)
        {
            var compiled = predicate.Compile();
            return _entities.Values.FirstOrDefault(compiled);
        }

        public virtual T Add(T entity)
        {
            ValidateEntity(entity);

            var id = GetNextId();
            entity.Id = id;
            SetTimestamps(entity, isNew: true);

            if (!_entities.TryAdd(id, entity))
            {
                throw new InvalidOperationException($"Failed to add entity with ID {id}");
            }

            return entity;
        }

        public virtual IEnumerable<T> AddRange(IEnumerable<T> entities)
        {
            var addedEntities = new List<T>();

            foreach (var entity in entities)
            {
                addedEntities.Add(Add(entity));
            }

            return addedEntities;
        }

        public virtual T? Update(int id, T entity)
        {
            ValidateEntity(entity);

            if (!_entities.TryGetValue(id, out var existingEntity))
                return null;

            UpdateEntityProperties(existingEntity, entity);
            SetTimestamps(existingEntity, isNew: false);

            return existingEntity;
        }

        public virtual T? Update(T entity)
        {
            if (entity?.Id <= 0)
                throw new ArgumentException("Entity must have a valid ID", nameof(entity));

            return Update(entity.Id, entity);
        }

        public virtual bool Delete(int id)
        {
            return _entities.TryRemove(id, out _);
        }

        public virtual int DeleteRange(Expression<Func<T, bool>> predicate)
        {
            var compiled = predicate.Compile();
            var idsToDelete = _entities.Values
                .Where(compiled)
                .Select(x => x.Id)
                .ToList();

            var deletedCount = 0;
            foreach (var id in idsToDelete)
            {
                if (_entities.TryRemove(id, out _))
                    deletedCount++;
            }

            return deletedCount;
        }

        public virtual int Count()
        {
            return _entities.Count;
        }

        public virtual int Count(Expression<Func<T, bool>> predicate)
        {
            var compiled = predicate.Compile();
            return _entities.Values.Count(compiled);
        }

        public virtual bool Exists(int id)
        {
            return _entities.ContainsKey(id);
        }

        public virtual bool Any(Expression<Func<T, bool>> predicate)
        {
            var compiled = predicate.Compile();
            return _entities.Values.Any(compiled);
        }

        public virtual void Clear()
        {
            _entities.Clear();
        }

        protected static int GetNextId()
        {
            lock (_idLock)
            {
                return _nextId++;
            }
        }

        private static void SetTimestamps(T entity, bool isNew)
        {
            var now = DateTime.UtcNow;

            if (isNew)
            {
                entity.CreatedAt = now;
            }
        }

        protected virtual void UpdateEntityProperties(T existing, T updated)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite &&
                           p.Name != nameof(IEntity.Id) &&
                           p.Name != nameof(IEntity.CreatedAt));

            foreach (var property in properties)
            {
                var value = property.GetValue(updated);
                if (value != null)
                {
                    property.SetValue(existing, value);
                }
            }
        }

        protected virtual void ValidateEntity(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Use DataAnnotations validation if available
            var validationContext = new ValidationContext(entity);
            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(entity, validationContext, validationResults, true))
            {
                var errors = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
                throw new ValidationException($"Entity validation failed: {errors}");
            }
        }
    }
}
