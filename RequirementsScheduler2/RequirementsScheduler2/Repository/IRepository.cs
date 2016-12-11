using System;
using System.Collections.Generic;

namespace RequirementsScheduler2.Repository
{
    interface IRepository<T> where T : IRepositoryModel
    {
        IEnumerable<T> Get();
        T Get(int id);
        T Get(Func<T, bool> predicate);
        void Add(T value);
    }
}
