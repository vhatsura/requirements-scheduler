namespace RequirementsScheduler.DAL.Repository
{
    public interface IRepository<TEntity, in TKey> : IReadOnlyRepository<TEntity, TKey>
        where TEntity : class
    {
        TEntity Add(TEntity entity);

        //TEntity AddWithoutIdentity(TEntity entity);
        bool Delete(TKey id);
        TEntity Update(TKey id, TEntity entity);
    }
}