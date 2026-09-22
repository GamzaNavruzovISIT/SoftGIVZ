using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Soft_Givz.Models;
using Soft_Givz.Repositories;

namespace Soft_Givz.ViewModels;

public partial class TableEditorViewModel : ViewModelBase
{
    private readonly UniversalRepository _repo;

    public string TableName { get; }

    public ObservableCollection<DbRow> Rows { get; } = new();

    // Список колонок для отображения (по порядку)
    public string[] ColumnNames { get; }

    [ObservableProperty] private string _statusMessage = "";

    public TableEditorViewModel(UniversalRepository repo, string tableName, string[] columns)
    {
        _repo = repo;
        TableName = tableName;
        ColumnNames = columns;
    }

    public TableEditorViewModel() : this(null!, "", Array.Empty<string>()) { }

    public async Task LoadAsync()
    {
        try
        {
            var rows = await _repo.GetAllAsync(TableName);
            Rows.Clear();
            foreach (var r in rows) Rows.Add(r);
            StatusMessage = $"Загружено: {Rows.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка: " + ex.Message;
        }
    }

    public async Task UpdateCellAsync(int id, string column, object? value)
    {
        try
        {
            await _repo.UpdateCellAsync(TableName, id, column, value);
            StatusMessage = $"Обновлено: {column}";
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка: " + ex.Message;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync() => await LoadAsync();

    [RelayCommand]
    private async Task DeleteSelectedAsync(DbRow? row)
    {
        if (row == null) return;

        var idObj = row["id"];
        if (idObj == null) return;

        int id = Convert.ToInt32(idObj);
        await _repo.DeleteRowAsync(TableName, id);
        Rows.Remove(row);
        StatusMessage = $"Удалено id={id}";
    }
}