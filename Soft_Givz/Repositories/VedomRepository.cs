using Dapper;
using Soft_Givz.Data;
using Soft_Givz.Models;
using System.Threading.Tasks;

namespace Soft_Givz.Repositories;

public class VedomRepository
{
    private const string Schema = "migration";

    private readonly IDbConnectionFactory _factory;

    public string TableName { get; }

    public VedomRepository(IDbConnectionFactory factory, string tableName)
    {
        _factory = factory;
        TableName = tableName;
    }

    public async Task InsertAsync(Vedom1 v)
    {
        var sql = $@"
            INSERT INTO {Schema}.{TableName}
            (v_gmd, v_pach, v_stan, v_nved, v_tiploc, v_otp, v_uchastok, v_gruz,
             v_kvag1, v_sokr1, v_kvag2, v_sokr2, v_kvag3, v_sokr3, v_kvag4, v_sokr4,
             v_prostoi, v_trab, v_data,
             v_data_vv, v_time_vv)
            VALUES
            (@VGmd, @VPach, @VStan, @VNved, @VTiploc, @VOtp, @VUchastok, @VGruz,
             @VKvag1, @VSokr1, @VKvag2, @VSokr2, @VKvag3, @VSokr3, @VKvag4, @VSokr4,
             @VProstoi, @VTrab, @VData,
             @VDataVv, @VTimeVv)";

        using var conn = _factory.Create();
        await conn.ExecuteAsync(sql, v);
    }
}