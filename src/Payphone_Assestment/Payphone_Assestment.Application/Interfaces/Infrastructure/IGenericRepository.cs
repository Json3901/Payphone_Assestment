using Payphone_Assestment.Domain.Entities;

namespace Payphone_Assestment.Application.Interfaces.Infrastructure;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetByFilterAsync(Dictionary<string, object> filters);
    Task<int> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> SoftDeleteAsync(int id);
}