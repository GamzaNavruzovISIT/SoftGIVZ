using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Soft_Givz.Repositories;
using Soft_Givz.Services;
using Soft_Givz.Views;

namespace Soft_Givz.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly DorogaRepository _doroga1Repo;
    private readonly DorogaRepository _doroga2Repo;
    private readonly MergeService _mergeService;

    public MainWindowViewModel(
        DorogaRepository doroga1Repo,
        DorogaRepository doroga2Repo,
        MergeService mergeService)
    {
        _doroga1Repo = doroga1Repo;
        _doroga2Repo = doroga2Repo;
        _mergeService = mergeService;
    }

    // Пустой конструктор — для дизайнера XAML
    public MainWindowViewModel() : this(null!, null!, null!) { }

    [ObservableProperty] private bool _isEntryMenuVisible;

    // ===== Верхняя кнопка =====
    [RelayCommand]
    private void ToggleEntryMenu() => IsEntryMenuVisible = !IsEntryMenuVisible;

    // ===== Ведомости (пока заглушки) =====
    [RelayCommand] private void EnterVedomost1() { }
    [RelayCommand] private void EnterVedomost2() { }
    [RelayCommand] private void CompareVedom() { }
    [RelayCommand] private void ClearVedom() { }

    // ===== Дороги =====
    [RelayCommand]
    private void EnterDoroga1() => OpenDorogaInput("Ввод Дорога 1", _doroga1Repo);

    [RelayCommand]
    private void EnterDoroga2() => OpenDorogaInput("Ввод Дорога 2", _doroga2Repo);

    [RelayCommand]
    private async Task CompareDorogaAsync()
    {
        try
        {
            var result = await _mergeService.MergeDorogaAsync();
            ShowMergeResult("Сравнение дорог", result);
        }
        catch (Exception ex)
        {
            ShowInfo("Ошибка", ex.Message);
        }
    }

    [RelayCommand]
    private async Task ClearDorogaAsync()
    {
        try
        {
            await _mergeService.ClearDorogaTablesAsync();
            ShowInfo("Очистка", "Таблицы doroga1 и doroga2 очищены.");
        }
        catch (Exception ex)
        {
            ShowInfo("Ошибка", ex.Message);
        }
    }

    // ===== Открытие окна ввода =====
    private void OpenDorogaInput(string title, DorogaRepository repo)
    {
        var vm = new DorogaInputViewModel(repo);
        var window = new DorogaInputWindow { DataContext = vm, Title = title };
        vm.SaveSucceeded += () => window.Close();
        window.Show();
    }

    // ===== Диалог с ошибками слияния =====
    private void ShowMergeResult(string title, MergeResult result)
    {
        if (result.Success)
        {
            ShowInfo(title, result.Message);
            return;
        }

        var errorsText = string.Join("\n", result.Errors);

        var textBox = new TextBox
        {
            Text = errorsText,
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Height = 300,
            Width = 520,
            Foreground = Brushes.DarkRed
        };

        var okButton = new Button
        {
            Content = "Понятно",
            HorizontalAlignment = HorizontalAlignment.Right,
            Padding = new Thickness(15, 8)
        };

        var panel = new StackPanel { Spacing = 10, Margin = new Thickness(15) };
        panel.Children.Add(new TextBlock
        {
            Text = result.Message,
            FontWeight = FontWeight.Bold,
            TextWrapping = TextWrapping.Wrap
        });
        panel.Children.Add(textBox);
        panel.Children.Add(okButton);

        var win = new Window
        {
            Title = title,
            Width = 580,
            Height = 440,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = panel
        };
        okButton.Click += (_, _) => win.Close();
        win.ShowDialog(GetMainWindow());
    }

    // ===== Информационный диалог =====
    private void ShowInfo(string title, string message)
    {
        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Right,
            Padding = new Thickness(15, 8)
        };

        var panel = new StackPanel { Spacing = 10, Margin = new Thickness(15) };
        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap
        });
        panel.Children.Add(okButton);

        var win = new Window
        {
            Title = title,
            Width = 400,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = panel
        };
        okButton.Click += (_, _) => win.Close();
        win.ShowDialog(GetMainWindow());
    }

    private static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime
            is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }
}