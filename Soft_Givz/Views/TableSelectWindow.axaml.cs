using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Soft_Givz.Views;

public partial class TableSelectWindow : Window
{
    public string? SelectedTable { get; private set; }

    public TableSelectWindow()
    {
        InitializeComponent();
    }

    private void OnTableClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string table)
        {
            SelectedTable = table;
            Close(SelectedTable);
        }
    }
}