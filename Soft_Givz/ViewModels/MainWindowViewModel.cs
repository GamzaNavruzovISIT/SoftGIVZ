using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Soft_Givz.Repositories;
using Soft_Givz.Views;

namespace Soft_Givz.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly DorogaRepository _doroga1Repo;
    private readonly DorogaRepository _doroga2Repo;

    public MainWindowViewModel(DorogaRepository doroga1Repo, DorogaRepository doroga2Repo)
    {
        _doroga1Repo = doroga1Repo;
        _doroga2Repo = doroga2Repo;
    }

    // Пустой конструктор — для дизайнера XAML
    public MainWindowViewModel() : this(null!, null!) { }

    [ObservableProperty] private bool _isEntryMenuVisible;

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

    [RelayCommand] private void CompareDoroga() { }
    [RelayCommand] private void ClearDoroga() { }

    private void OpenDorogaInput(string title, DorogaRepository repo)
    {
        var vm = new DorogaInputViewModel(repo);
        var window = new DorogaInputWindow { DataContext = vm, Title = title };
        vm.SaveSucceeded += () => window.Close();
        window.Show();
    }
}