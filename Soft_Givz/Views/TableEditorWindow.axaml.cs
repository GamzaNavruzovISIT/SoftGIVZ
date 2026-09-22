using System;
using Avalonia.Controls;
using Avalonia.Data;
using Soft_Givz.Models;
using Soft_Givz.ViewModels;

namespace Soft_Givz.Views;

public partial class TableEditorWindow : Window
{
    public TableEditorWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is not TableEditorViewModel vm) return;

        // Очищаем и строим колонки вручную
        DataGridControl.Columns.Clear();

        foreach (var col in vm.ColumnNames)
        {
            // id — только для чтения
            bool isReadOnly = col.Equals("id", StringComparison.OrdinalIgnoreCase);

            var column = new DataGridTextColumn
            {
                Header = col,
                IsReadOnly = isReadOnly,
                Binding = new Binding($"[{col}]")
            };
            DataGridControl.Columns.Add(column);
        }
    }

    private async void OnCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit) return;
        if (DataContext is not TableEditorViewModel vm) return;
        if (e.Row.DataContext is not DbRow row) return;

        // Получаем id
        var idObj = row["id"];
        if (idObj == null) return;
        int id = Convert.ToInt32(idObj);

        // Какая колонка редактируется
        var column = e.Column.Header?.ToString();
        if (string.IsNullOrEmpty(column)) return;

        // Какое значение пользователь ввёл
        var newValue = row[column];

        // Сохраняем в БД
        await vm.UpdateCellAsync(id, column, newValue);
    }
}