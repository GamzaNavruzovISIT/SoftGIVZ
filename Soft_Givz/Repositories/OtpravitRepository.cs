using Dapper;
using Soft_Givz.Data;
using Soft_Givz.Models;
using System.Threading.Tasks;

namespace Soft_Givz.Repositories;

public class OtpravitRepository
{
    private const string Schema = "migration";
    private readonly IDbConnectionFactory _factory;

    public OtpravitRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    // Проверка: есть ли код в справочнике
    public async Task<bool> ExistsAsync(string kod)
    {
        if (string.IsNullOrWhiteSpace(kod)) return false;

        using var conn = _factory.Create();
        var count = await conn.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM {Schema}.otpravit WHERE otp_kod = @kod",
            new { kod });

        return count > 0;
    }
}