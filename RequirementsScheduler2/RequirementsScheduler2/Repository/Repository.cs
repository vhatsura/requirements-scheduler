using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace RequirementsScheduler2.Repository
{
    public abstract class Repository<T> : IRepository<T>
        where T : IRepositoryModel
    {
        protected static readonly BlockingCollection<T> ModelsCollection = new BlockingCollection<T>();
        private static int lastModelId = 1;

        public IEnumerable<T> Get()
        {
            return ModelsCollection.ToImmutableList();
        }

        public T Get(int id)
        {
            return ModelsCollection.FirstOrDefault(model => model.Id == id);
        }

        private static readonly object syncObject = new object();

        public void Add(T value)
        {
            lock (syncObject)
            {
                value.Id = lastModelId;
                ModelsCollection.Add(value);
                Interlocked.Increment(ref lastModelId);
            }
        }

        public T Get(Func<T, bool> predicate)
        {
            if(predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return ModelsCollection.FirstOrDefault(predicate);
        }
    }
}
