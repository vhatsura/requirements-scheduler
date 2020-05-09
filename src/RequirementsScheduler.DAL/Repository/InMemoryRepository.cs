using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

namespace RequirementsScheduler.DAL.Repository
{
    public abstract class InMemoryRepository<TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : class, IRepositoryModel<TKey>
    {
        protected static readonly ConcurrentDictionary<TKey, TEntity> ModelsCollection =
            new ConcurrentDictionary<TKey, TEntity>();

        protected abstract TKey NextId { get; }

        public IEnumerable<TEntity> Get() => ModelsCollection.Values.ToImmutableList();

        public IEnumerable<TEntity> GetWith(Expression<Func<TEntity, object>> selector) =>
            ModelsCollection.Values.ToImmutableList();

        public TEntity GetWith(TKey id, Expression<Func<TEntity, object>> selector)
        {
            TEntity value;
            ModelsCollection.TryGetValue(id, out value);
            return value;
        }

        public TEntity Get(TKey id)
        {
            TEntity value;
            ModelsCollection.TryGetValue(id, out value);
            return value;
        }

        public TEntity Add(TEntity value)
        {
            BeforeAdd(value);

            value.Id = NextId;

            ModelsCollection.TryAdd(value.Id, value);
            return Get(value.Id);
        }

        public IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var filteredCollection = ModelsCollection
                .Select(pair => pair.Value)
                .Where(filter.Compile())
                .ToList();

            return filteredCollection.Any() ? filteredCollection : Enumerable.Empty<TEntity>();
        }

        public bool Delete(TKey id) => ModelsCollection.TryRemove(id, out var entity);

        public TEntity Update(TKey id, TEntity entity)
        {
            ModelsCollection.AddOrUpdate(id, entity, (k, v) => entity);
            return Get(id);
        }

        protected static void AddDefaultValue(params TEntity[] values)
        {
            foreach (var value in values)
            {
                if (Equals(value.Id, default(TKey)))
                {
                    throw new ArgumentOutOfRangeException();
                }

                ModelsCollection.TryAdd(value.Id, value);
            }
        }

        protected virtual void BeforeAdd(TEntity value)
        {
        }

        public TEntity AddWithoutIdentity(TEntity entity)
        {
            BeforeAdd(entity);

            ModelsCollection.TryAdd(entity.Id, entity);
            return Get(entity.Id);
        }
    }
}
