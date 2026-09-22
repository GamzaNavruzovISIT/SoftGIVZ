using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Soft_Givz.Data;
using Soft_Givz.Models;

namespace Soft_Givz.Repositories;

public class UniversalRepository
{
    private const string Schema = "migration";
    private readonly IDbConnectionFactory _factory;

    public UniversalRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<List<DbRow>> GetAllAsync(string tableName)
    {
        using var conn = _factory.Create();

        // Читаем как IEnumerable<dynamic>, превращаем в DbRow
        var raw = await conn.QueryAsync($"SELECT * FROM {Schema}.{tableName}");

        var result = new List<DbRow>();
        foreach (var item in raw)
        {
            var dict = (IDictionary<string, object>)item;
            var row = new DbRow();
            foreach (var kv in dict)
                row[kv.Key] = kv.Value;
            result.Add(row);
        }
        return result;
    }

    // Универсальный UPDATE по id: меняем ОДНУ колонку
    public async Task UpdateCellAsync(string tableName, int id, string column, object? value)
    {
        using var conn = _factory.Create();
        var sql = $"UPDATE {Schema}.{tableName} SET {column} = @Value WHERE id = @Id";
        await conn.ExecuteAsync(sql, new { Value = value, Id = id });
    }

    // Удаление строки
    public async Task DeleteRowAsync(string tableName, int id)
    {
        using var conn = _factory.Create();
        await conn.ExecuteAsync(
            $"DELETE FROM {Schema}.{tableName} WHERE id = @Id", new { Id = id });
    }

    // Полный список таблиц (хардкод)
    public static readonly string[] TableNames =
    {
        "doroga1", "doroga2", "doroga",
        "vedom1", "vedom2", "vedom"
    };
}