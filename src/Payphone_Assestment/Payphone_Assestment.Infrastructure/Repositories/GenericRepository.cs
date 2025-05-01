using System.Data;
using Dapper;
using Payphone_Assestment.Application.Interfaces.Infrastructure;
using Payphone_Assestment.Domain.Entities;

namespace Payphone_Assestment.Infrastructure.Repositories;

public class GenericRepository<T>(IDbConnection connection, IDbTransaction? transaction = null)
    : IGenericRepository<T>
    where T : BaseEntity
{
    private readonly string _tableName = typeof(T).Name + "s";

    public async Task<T?> GetByIdAsync(int id)
    {
        var sql = $"SELECT * FROM {_tableName} WHERE Id = @Id AND IsDeleted = 0";
        return await connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id }, transaction);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var sql = $"SELECT * FROM {_tableName} WHERE IsDeleted = 0";
        return await connection.QueryAsync<T>(sql, transaction: transaction);
    }
    
    public async Task<IEnumerable<T>> GetByFilterAsync(Dictionary<string, object> filters)
    {
        var whereClauses = filters.Select(f => $"{f.Key} = @{f.Key}");
        var whereSql = string.Join(" AND ", whereClauses);

        var sql = $"SELECT * FROM {_tableName} WHERE {whereSql} AND IsDeleted = 0";
        return await connection.QueryAsync<T>(sql, filters, transaction);
    }

    public async Task<T> AddAsync(T entity)
    {
        entity.IsDisabled = false;
        entity.IsDeleted = false;
        entity.CreatedAt = DateTime.UtcNow;
        
        var props = typeof(T).GetProperties()
            .Where(p => p.Name != "Id")
            .Select(p => p.Name);

        var columns = string.Join(", ", props);
        var values = string.Join(", ", props.Select(p => "@" + p));

        var sql = $"INSERT INTO {_tableName} ({columns}) VALUES ({values}); SELECT CAST(SCOPE_IDENTITY() as int);";

        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity, transaction);
        
        var sqlSelect = $"SELECT * FROM {_tableName} WHERE Id = @Id AND IsDeleted = 0";
        var result = await connection.QueryFirstOrDefaultAsync<T>(sqlSelect, new {  entity.Id }, transaction);

        return result ?? throw new Exception("Error al recuperar el registro insertado.");

    }

    public async Task<bool> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;

        var props = typeof(T).GetProperties()
            .Where(p => p.Name != "Id" && p.Name != "CreatedAt")
            .Select(p => $"{p.Name} = @{p.Name}");

        var sql = $"UPDATE {_tableName} SET {string.Join(", ", props)} WHERE Id = @Id";

        var result = await connection.ExecuteAsync(sql, entity, transaction);
        return result > 0;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var sql = $"UPDATE {_tableName} SET IsDisabled=1,  IsDeleted = 1, UpdatedAt = @Now WHERE Id = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = id, Now = DateTime.UtcNow }, transaction);
        return result > 0;
    }
}