using System.Data;
using Payphone_Assestment.Domain.Entities;

namespace Payphone_Assestment.Application.Interfaces.Infrastructure;

public interface IUnitOfWork : IDisposable
{
    IDbTransaction? CurrentTransaction { get; }
    IGenericRepository<T> Repository<T>() where T : BaseEntity;
    void BeginTransaction();
    Task CommitAsync();
    void Rollback();
}