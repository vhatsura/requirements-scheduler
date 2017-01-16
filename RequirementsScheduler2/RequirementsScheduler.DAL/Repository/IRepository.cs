using System;
using System.Collections.Generic;
using RequirementsScheduler.Core.Model;

namespace RequirementsScheduler.DAL.Repository
{
    public interface IRepository<T> where T : IRepositoryModel
    {
        IEnumerable<T> Get();
        T Get(int id);
        void Update(T value);
        IEnumerable<T> Get(Func<T, bool> predicate);
        void Add(T value);
    }
}
