using System.Linq.Expressions;

namespace TaskTracker.Api.Services;

public interface IDatabaseService<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(string id, T entity);
    Task DeleteAsync(string id);
} 