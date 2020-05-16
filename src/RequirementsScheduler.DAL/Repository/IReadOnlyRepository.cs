using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RequirementsScheduler.DAL.Repository
{
    public interface IReadOnlyRepository<TEntity, in TKey> where TEntity : class
    {
        IEnumerable<TEntity> Get();
        IEnumerable<TEntity> GetWith(Expression<Func<TEntity, object>> selector);
        TEntity GetWith(TKey id, Expression<Func<TEntity, object>> selector);
        IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter);
        TEntity Get(TKey id);
    }
}
