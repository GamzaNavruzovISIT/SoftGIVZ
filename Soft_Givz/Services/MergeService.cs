using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Soft_Givz.Data;

namespace Soft_Givz.Services;

public class MergeResult
{
    public bool Success { get; init; }
    public int MergedCount { get; init; }
    public List<string> Errors { get; init; } = new();
    public string Message { get; init; } = "";

    public static MergeResult Ok(int count) =>
        new() { Success = true, MergedCount = count, Message = $"Слито записей: {count}" };

    public static MergeResult Fail(List<string> errors) =>
        new()
        {
            Success = false,
            Errors = errors,
            Message = $"Ошибок: {errors.Count}. Слияние отменено."
        };
}

public class MergeService
{
    private const string Schema = "migration";

    private readonly IDbConnectionFactory _factory;

    public MergeService(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    // ============================================================
    // ДОРОГИ
    // ============================================================
    public Task<MergeResult> MergeDorogaAsync() =>
        MergeGenericAsync(
            src1: "doroga1", src2: "doroga2", dst: "doroga",
            allColumns: DorogaColumns,
            keyColumns: new[] { "d_gmd", "d_pach", "d_nakl" },
            label: "накладная");

    public Task ClearDorogaTablesAsync() =>
        ClearTablesAsync("doroga1", "doroga2");

    // ============================================================
    // ВЕДОМОСТИ
    // ============================================================
    public Task<MergeResult> MergeVedomAsync() =>
        MergeGenericAsync(
            src1: "vedom1", src2: "vedom2", dst: "vedom",
            allColumns: VedomColumns,
            keyColumns: new[] { "v_gmd", "v_pach", "v_nved" },
            label: "ведомость");

    public Task ClearVedomTablesAsync() =>
        ClearTablesAsync("vedom1", "vedom2");

    // ============================================================
    // ОЧИСТКА ОБЩАЯ
    // ============================================================
    public async Task ClearTablesAsync(string t1, string t2)
    {
        using var conn = _factory.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            await conn.ExecuteAsync(
                $"TRUNCATE TABLE {Schema}.{t1} RESTART IDENTITY", transaction: tx);
            await conn.ExecuteAsync(
                $"TRUNCATE TABLE {Schema}.{t2} RESTART IDENTITY", transaction: tx);
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    // ============================================================
    // ОБЩАЯ ЛОГИКА СЛИЯНИЯ
    // ============================================================
    private async Task<MergeResult> MergeGenericAsync(
        string src1, string src2, string dst,
        string[] allColumns, string[] keyColumns, string label)
    {
        using var conn = _factory.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            var cols = string.Join(", ", allColumns);

            var rows1 = (await conn.QueryAsync(
                $"SELECT {cols} FROM {Schema}.{src1}", transaction: tx)).ToList();
            var rows2 = (await conn.QueryAsync(
                $"SELECT {cols} FROM {Schema}.{src2}", transaction: tx)).ToList();

            var errors = new List<string>();
            var toMerge = new List<IDictionary<string, object?>>();

            // Прямой проход
            foreach (var row1 in rows1)
            {
                var dict1 = (IDictionary<string, object?>)row1;
                var key1 = BuildKey(dict1, keyColumns);

                var pair = rows2
                    .Cast<IDictionary<string, object?>>()
                    .FirstOrDefault(r => BuildKey(r, keyColumns) == key1);

                if (pair == null)
                {
                    errors.Add($"Нет пары для {label} {FormatKey(dict1, keyColumns)}");
                    continue;
                }

                var compareCols = allColumns
                    .Where(c => !keyColumns.Contains(c))
                    .ToArray();

                var diff = CompareRows(dict1, pair, compareCols);
                if (diff.Count > 0)
                {
                    errors.Add(
                        $"{label} {FormatKey(dict1, keyColumns)}: " +
                        $"расхождения в полях: {string.Join(", ", diff)}");
                    continue;
                }

                toMerge.Add(dict1);
            }

            // Обратный проход
            foreach (var row2 in rows2)
            {
                var dict2 = (IDictionary<string, object?>)row2;
                var key2 = BuildKey(dict2, keyColumns);

                bool found = rows1
                    .Cast<IDictionary<string, object?>>()
                    .Any(r => BuildKey(r, keyColumns) == key2);

                if (!found)
                {
                    errors.Add(
                        $"Нет пары для {label} {FormatKey(dict2, keyColumns)} (в источнике 2)");
                }
            }

            // Если есть ошибки — откат
            if (errors.Count > 0)
            {
                tx.Rollback();
                return MergeResult.Fail(errors);
            }

            // Слияние
            var paramNames = allColumns.Select(c => "@" + c).ToArray();
            var insertSql = $@"
                INSERT INTO {Schema}.{dst} ({cols})
                VALUES ({string.Join(", ", paramNames)})";

            foreach (var row in toMerge)
            {
                await conn.ExecuteAsync(insertSql, (object)row, tx);
            }

            // Очистка
            await conn.ExecuteAsync(
                $"TRUNCATE TABLE {Schema}.{src1} RESTART IDENTITY", transaction: tx);
            await conn.ExecuteAsync(
                $"TRUNCATE TABLE {Schema}.{src2} RESTART IDENTITY", transaction: tx);

            tx.Commit();
            return MergeResult.Ok(toMerge.Count);
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    // ============================================================
    // СПИСКИ ПОЛЕЙ
    // ============================================================

    private static readonly string[] DorogaColumns = new[]
    {
        "d_gmd", "d_pach", "d_nakl",
        "d_otp_v", "d_stotp", "d_stnazn", "d_gruz",
        "d_psila", "d_ves", "d_poluhat", "d_data",
        "d_vag1", "d_vag2", "d_vag3", "d_vag4"
    };

    private static readonly string[] VedomColumns = new[]
    {
        "v_gmd", "v_pach", "v_stan", "v_nved", "v_tiploc", "v_otp",
        "v_uchastok", "v_gruz",
        "v_kvag1", "v_sokr1", "v_kvag2", "v_sokr2",
        "v_kvag3", "v_sokr3", "v_kvag4", "v_sokr4",
        "v_prostoi", "v_trab", "v_data"
    };

    // ============================================================
    // ВСПОМОГАТЕЛЬНЫЕ
    // ============================================================

    private static string BuildKey(IDictionary<string, object?> row, string[] keyCols)
        => string.Join("|", keyCols.Select(c =>
            row.TryGetValue(c, out var v) ? v?.ToString() ?? "" : ""));

    private static string FormatKey(IDictionary<string, object?> row, string[] keyCols)
        => string.Join(", ", keyCols.Select(c =>
            $"{c}={(row.TryGetValue(c, out var v) ? v?.ToString() ?? "" : "")}"));

    private static List<string> CompareRows(
        IDictionary<string, object?> a,
        IDictionary<string, object?> b,
        string[] columns)
    {
        var diffs = new List<string>();
        foreach (var col in columns)
        {
            a.TryGetValue(col, out var va);
            b.TryGetValue(col, out var vb);

            if (!AreEqual(va, vb))
                diffs.Add(col);
        }
        return diffs;
    }

    private static bool AreEqual(object? a, object? b)
    {
        var aEmpty = a == null || (a is string sa && string.IsNullOrWhiteSpace(sa));
        var bEmpty = b == null || (b is string sb && string.IsNullOrWhiteSpace(sb));

        if (aEmpty && bEmpty) return true;
        if (aEmpty || bEmpty) return false;

        if (a is decimal da && b is decimal db) return da == db;
        if (a is int ia && b is int ib) return ia == ib;
        if (a is long la && b is long lb) return la == lb;

        return string.Equals(a.ToString()?.Trim(), b.ToString()?.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }
}