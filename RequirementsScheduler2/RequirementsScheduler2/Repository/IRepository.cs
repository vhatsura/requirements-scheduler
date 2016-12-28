using System;
using System.Collections.Generic;

namespace RequirementsScheduler2.Repository
{
    interface IRepository<T> where T : IRepositoryModel
    {
        IEnumerable<T> Get();
        T Get(int id);
        void Update(T value);
        IEnumerable<T> Get(Func<T, bool> predicate);
        void Add(T value);
    }
}
