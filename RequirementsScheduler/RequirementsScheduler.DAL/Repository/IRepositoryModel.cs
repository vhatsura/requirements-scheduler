namespace RequirementsScheduler.DAL.Repository
{
    public interface IRepositoryModel<TKey>
    {
        TKey Id { get; set; }
    }
}