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
    private readonly VedomRepository _vedom1Repo;
    private readonly VedomRepository _vedom2Repo;
    private readonly MergeService _mergeService;
    private readonly UniversalRepository _universalRepo;
    private readonly VagonRepository _vagonRepo;
    private readonly OtpravitRepository _otpravitRepo;

    public MainWindowViewModel(
        DorogaRepository doroga1Repo,
        DorogaRepository doroga2Repo,
        VedomRepository vedom1Repo,
        VedomRepository vedom2Repo,
        MergeService mergeService,
        UniversalRepository universalRepo,
        VagonRepository vagonRepo,
        OtpravitRepository otpravitRepo)
    {
        _doroga1Repo = doroga1Repo;
        _doroga2Repo = doroga2Repo;
        _vedom1Repo = vedom1Repo;
        _vedom2Repo = vedom2Repo;
        _mergeService = mergeService;
        _universalRepo = universalRepo;
        _vagonRepo = vagonRepo;
        _otpravitRepo = otpravitRepo;
    }

    public MainWindowViewModel()
        : this(null!, null!, null!, null!, null!, null!, null!, null!) { }

    [ObservableProperty] private bool _isEntryMenuVisible;

    [RelayCommand]
    private void ToggleEntryMenu() => IsEntryMenuVisible = !IsEntryMenuVisible;

    // ===== Ведомости =====
    [RelayCommand]
    private void EnterVedomost1() => OpenVedomInput("Ввод Ведомость 1", _vedom1Repo);

    [RelayCommand]
    private void EnterVedomost2() => OpenVedomInput("Ввод Ведомость 2", _vedom2Repo);

    [RelayCommand]
    private async Task CompareVedomAsync()
    {
        try
        {
            var result = await _mergeService.MergeVedomAsync();
            ShowMergeResult("Сравнение ведомостей", result);
        }
        catch (Exception ex)
        {
            ShowInfo("Ошибка", ex.Message);
        }
    }

    [RelayCommand]
    private async Task ClearVedomAsync()
    {
        try
        {
            await _mergeService.ClearVedomTablesAsync();
            ShowInfo("Очистка", "Таблицы vedom1 и vedom2 очищены.");
        }
        catch (Exception ex)
        {
            ShowInfo("Ошибка", ex.Message);
        }
    }

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

    // ===== Работа с БД =====
    [RelayCommand]
    private async Task OpenDbEditorAsync()
    {
        var selectWindow = new TableSelectWindow();
        var table = await selectWindow.ShowDialog<string?>(GetMainWindow());

        if (string.IsNullOrEmpty(table)) return;

        string[] columns = table.StartsWith("doroga")
            ? new[]
            {
                "id", "d_gmd", "d_pach", "d_nakl", "d_otp_v", "d_stotp", "d_stnazn",
                "d_gruz", "d_psila", "d_ves", "d_poluhat", "d_data",
                "d_vag1", "d_vag2", "d_vag3", "d_vag4",
                "d_data_vv", "d_time_vv"
            }
            : new[]
            {
                "id", "v_gmd", "v_pach", "v_stan", "v_nved", "v_tiploc", "v_otp",
                "v_uchastok", "v_gruz",
                "v_kvag1", "v_sokr1", "v_kvag2", "v_sokr2",
                "v_kvag3", "v_sokr3", "v_kvag4", "v_sokr4",
                "v_prostoi", "v_trab", "v_data",
                "v_data_vv", "v_time_vv"
            };

        var editorVm = new TableEditorViewModel(_universalRepo, table, columns);
        var editorWindow = new TableEditorWindow { DataContext = editorVm };
        await editorVm.LoadAsync();
        editorWindow.Show();
    }

    // ===== Открытие окон ввода =====
    private void OpenDorogaInput(string title, DorogaRepository repo)
    {
        var vm = new DorogaInputViewModel(repo, _otpravitRepo);
        var window = new DorogaInputWindow { DataContext = vm, Title = title };
        vm.SaveSucceeded += () => window.Close();
        window.Show();
    }

    private void OpenVedomInput(string title, VedomRepository repo)
    {
        var vm = new VedomInputViewModel(repo, _vagonRepo, _otpravitRepo);
        var window = new VedomInputWindow { DataContext = vm, Title = title };
        vm.SaveSucceeded += () => window.Close();
        _ = vm.LoadVagonsAsync();
        window.Show();
    }

    // ===== Диалоги =====
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