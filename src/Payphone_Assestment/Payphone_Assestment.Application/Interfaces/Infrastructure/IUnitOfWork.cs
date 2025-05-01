using Payphone_Assestment.Domain.Entities;

namespace Payphone_Assestment.Application.Interfaces.Infrastructure;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T> Repository<T>() where T : BaseEntity;
    void BeginTransaction();
    Task CommitAsync();
    void Rollback();
}