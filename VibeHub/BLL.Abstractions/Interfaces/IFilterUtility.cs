namespace BLL.Abstractions.Interfaces;

public interface IFilterUtility
{
    Task<TEntity> Filter<TEntity>(TEntity entity) where TEntity : class, new();
    Task<List<TEntity>> FilterCollection<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, new();
}