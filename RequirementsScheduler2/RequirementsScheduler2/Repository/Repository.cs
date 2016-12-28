using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace RequirementsScheduler2.Repository
{
    public abstract class Repository<T> : IRepository<T>
        where T : class, IRepositoryModel
    {
        protected static readonly ConcurrentDictionary<int, T> ModelsCollection = new ConcurrentDictionary<int, T>();

        public IEnumerable<T> Get()
        {
            return ModelsCollection.Values.ToImmutableList();
        }

        public T Get(int id)
        {
            T value;
            ModelsCollection.TryGetValue(id, out value);
            return value;
        }

        public void Update(T value)
        {
            ModelsCollection.AddOrUpdate(value.Id, value, (k, v) => value);
        }

        // ReSharper disable StaticMemberInGenericType
        private static int lastModelId = 1;
        private static readonly object syncObject = new object();
        // ReSharper restore StaticMemberInGenericType

        protected virtual void BeforeAdd(T value) { }

        public void Add(T value)
        {
            BeforeAdd(value);

            lock (syncObject)
            {
                value.Id = lastModelId;
                Interlocked.Increment(ref lastModelId);
            }

            ModelsCollection.TryAdd(value.Id, value);
        }

        public IEnumerable<T> Get(Func<T, bool> predicate)
        {
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var filteredCollection = ModelsCollection.Where(predicate).ToList();

            return filteredCollection.Any() ? filteredCollection : Enumerable.Empty<T>();
        }
    }
}
