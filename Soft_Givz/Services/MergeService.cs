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
    // СЛИЯНИЕ ДОРОГ
    // ============================================================
    public async Task<MergeResult> MergeDorogaAsync()
    {
        using var conn = _factory.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            var cols = string.Join(", ", DorogaColumns);

            // 1. Читаем обе таблицы
            var rows1 = (await conn.QueryAsync(
                $"SELECT {cols} FROM {Schema}.doroga1", transaction: tx)).ToList();
            var rows2 = (await conn.QueryAsync(
                $"SELECT {cols} FROM {Schema}.doroga2", transaction: tx)).ToList();

            var errors = new List<string>();
            var toMerge = new List<IDictionary<string, object?>>();

            // 2. Прямой проход
            foreach (var row1 in rows1)
            {
                var dict1 = (IDictionary<string, object?>)row1;
                var key1 = BuildKey(dict1, KeyColumns);

                var pair = rows2
                    .Cast<IDictionary<string, object?>>()
                    .FirstOrDefault(r => BuildKey(r, KeyColumns) == key1);

                if (pair == null)
                {
                    errors.Add($"Нет пары для накладной {FormatKey(dict1, KeyLabelColumns)}");
                    continue;
                }

                var compareCols = DorogaColumns
                    .Where(c => !KeyColumns.Contains(c))
                    .ToArray();

                var diff = CompareRows(dict1, pair, compareCols);
                if (diff.Count > 0)
                {
                    errors.Add(
                        $"Накладная {FormatKey(dict1, KeyLabelColumns)}: " +
                        $"расхождения в полях: {string.Join(", ", diff)}");
                    continue;
                }

                // Данные берём из doroga1 — они идентичны doroga2 (сверили по всем полям)
                toMerge.Add(dict1);
            }

            // 3. Обратный проход — строки из 2, которых нет в 1
            foreach (var row2 in rows2)
            {
                var dict2 = (IDictionary<string, object?>)row2;
                var key2 = BuildKey(dict2, KeyColumns);

                bool found = rows1
                    .Cast<IDictionary<string, object?>>()
                    .Any(r => BuildKey(r, KeyColumns) == key2);

                if (!found)
                {
                    errors.Add(
                        $"Нет пары для накладной {FormatKey(dict2, KeyLabelColumns)} " +
                        $"(в источнике 2)");
                }
            }

            // 4. Если ошибки — откат
            if (errors.Count > 0)
            {
                tx.Rollback();
                return MergeResult.Fail(errors);
            }

            // 5. Сливаем в doroga
            var paramNames = DorogaColumns.Select(c => "@" + c).ToArray();
            var insertSql = $@"
                INSERT INTO {Schema}.doroga ({cols})
                VALUES ({string.Join(", ", paramNames)})";

            foreach (var row in toMerge)
            {
                await conn.ExecuteAsync(insertSql, (object)row, tx);
            }

            // 6. Чистим источники
            await conn.ExecuteAsync(
                $"TRUNCATE TABLE {Schema}.doroga1 RESTART IDENTITY", transaction: tx);
            await conn.ExecuteAsync(
                $"TRUNCATE TABLE {Schema}.doroga2 RESTART IDENTITY", transaction: tx);

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
    // ОЧИСТКА
    // ============================================================
    public async Task ClearDorogaTablesAsync()
    {
        using var conn = _factory.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            await conn.ExecuteAsync(
                $"TRUNCATE TABLE {Schema}.doroga1 RESTART IDENTITY", transaction: tx);
            await conn.ExecuteAsync(
                $"TRUNCATE TABLE {Schema}.doroga2 RESTART IDENTITY", transaction: tx);
            tx.Commit();
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

    private static readonly string[] KeyColumns = { "d_gmd", "d_pach", "d_nakl" };
    private static readonly string[] KeyLabelColumns = { "d_gmd", "d_pach", "d_nakl" };

    private static readonly string[] DorogaColumns = new[]
    {
        "d_gmd", "d_pach", "d_nakl",
        "d_otp_v", "d_stotp", "d_stnazn", "d_gruz",
        "d_psila", "d_ves", "d_poluhat", "d_data",
        "d_vag1", "d_vag2", "d_vag3", "d_vag4"
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