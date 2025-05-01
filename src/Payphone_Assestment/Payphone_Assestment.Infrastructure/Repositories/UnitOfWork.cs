using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Payphone_Assestment.Application.Interfaces.Infrastructure;
using Payphone_Assestment.Domain.Entities;

namespace Payphone_Assestment.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _transaction;
    private readonly Dictionary<string, object> _repositories = new();

    public UnitOfWork(IConfiguration configuration)
    {
        _connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        _connection.Open();
    }

    public void BeginTransaction()
    {
        if (_transaction == null)
            _transaction = _connection.BeginTransaction();
    }

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T).Name;

        if (_repositories.ContainsKey(type))
            return (IGenericRepository<T>)_repositories[type];

        var repo = new GenericRepository<T>(_connection, _transaction);
        _repositories[type] = repo;
        return repo;
    }

    public Task CommitAsync()
    {
        try
        {
            _transaction?.Commit();
        }
        catch
        {
            Rollback();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }

        return Task.CompletedTask;
    }

    public void Rollback()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _connection.Dispose();
    }
}