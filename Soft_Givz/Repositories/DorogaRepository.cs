using Dapper;
using Soft_Givz.Data;
using Soft_Givz.Models;
using System.Threading.Tasks;

namespace Soft_Givz.Repositories;

public class DorogaRepository
{
    private const string Schema = "migration";

    private readonly IDbConnectionFactory _factory;

    public string TableName { get; }

    public DorogaRepository(IDbConnectionFactory factory, string tableName)
    {
        _factory = factory;
        TableName = tableName;
    }

    public async Task InsertAsync(Doroga1 d)
    {
        var sql = $@"
            INSERT INTO {Schema}.{TableName}
            (d_gmd, d_pach, d_nakl, d_otp_v, d_stotp, d_stnazn, d_gruz,
             d_psila, d_ves, d_poluhat, d_data,
             d_vag1, d_vag2, d_vag3, d_vag4,
             d_data_vv, d_time_vv)
            VALUES
            (@DGmd, @DPach, @DNakl, @DOtpV, @DStotp, @DStnazn, @DGruz,
             @DPsila, @DVes, @DPoluhat, @DData,
             @DVag1, @DVag2, @DVag3, @DVag4,
             @DDataVv, @DTimeVv)";

        using var conn = _factory.Create();
        await conn.ExecuteAsync(sql, d);
    }
}