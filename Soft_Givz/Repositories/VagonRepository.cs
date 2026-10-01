using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Soft_Givz.Data;
using Soft_Givz.Models;

namespace Soft_Givz.Repositories;

public class VagonRepository
{
    private const string Schema = "migration";
    private readonly IDbConnectionFactory _factory;

    public VagonRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<Vagon>> GetAllAsync()
    {
        using var conn = _factory.Create();
        var sql = $@"
            SELECT
                vag_tip      AS VagTip,
                vag_not      AS VagNot,
                vag_ndo      AS VagNdo,
                vag_psila    AS VagPsila,
                vag_reft     AS VagReft,
                vag_cena     AS VagCena,
                vag_name     AS VagName,
                vag_sokr     AS VagSokr,
                vag_tar      AS VagTar,
                vag_c_s      AS VagCS,
                vag_t_s      AS VagTS,
                kom          AS Kom,
                tarif_grup   AS TarifGrup,
                data_izm     AS DataIzm
            FROM {Schema}.vagon
            ORDER BY vag_sokr";

        return await conn.QueryAsync<Vagon>(sql);
    }
}