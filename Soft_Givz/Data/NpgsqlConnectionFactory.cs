using System.Data;
using Npgsql;

namespace Soft_Givz.Data;

public class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection Create() => new NpgsqlConnection(_connectionString);
}