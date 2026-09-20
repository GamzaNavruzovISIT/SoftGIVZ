using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Soft_Givz.ViewModels;

namespace Soft_Givz.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // Видимость блока кнопок
    [ObservableProperty]
    private bool _isEntryMenuVisible;

    // === Верхняя левая кнопка ===
    [RelayCommand]
    private void ToggleEntryMenu() => IsEntryMenuVisible = !IsEntryMenuVisible;

    // ===== Ведомости =====
    [RelayCommand] private void EnterVedomost1() { /* TODO */ }
    [RelayCommand] private void EnterVedomost2() { /* TODO */ }
    [RelayCommand] private void CompareVedom() { /* TODO */ }
    [RelayCommand] private void ClearVedom() { /* TODO */ }

    // ===== Дороги =====
    [RelayCommand] private void EnterDoroga1() { /* TODO */ }
    [RelayCommand] private void EnterDoroga2() { /* TODO */ }
    [RelayCommand] private void CompareDoroga() { /* TODO */ }
    [RelayCommand] private void ClearDoroga() { /* TODO */ }
}