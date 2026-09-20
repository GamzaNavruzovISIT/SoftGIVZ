using System;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Soft_Givz.Models;
using Soft_Givz.Repositories;

namespace Soft_Givz.ViewModels;

public partial class DorogaInputViewModel : ViewModelBase
{
    private readonly DorogaRepository _repo;

    [ObservableProperty] private string _gmd = "";
    [ObservableProperty] private string _pachka = "";
    [ObservableProperty] private string _nakl = "";
    [ObservableProperty] private string _otpravitel = "";
    [ObservableProperty] private string _stanciyaOtpravleniya = "";
    [ObservableProperty] private string _stanciyaNaznacheniya = "";
    [ObservableProperty] private string _gruz = "";
    [ObservableProperty] private string _podyomnayaSila = "";
    [ObservableProperty] private string _poluchatel = "";
    [ObservableProperty] private string _chisloMest = "";
    [ObservableProperty] private string _dataDokumenta = "";

    // ===== Вагоны (4 поля в строчку) =====
    [ObservableProperty] private string _vagon1 = "";
    [ObservableProperty] private string _vagon2 = "";
    [ObservableProperty] private string _vagon3 = "";
    [ObservableProperty] private string _vagon4 = "";

    [ObservableProperty] private string _statusMessage = "";

    public event Action? SaveSucceeded;

    public DorogaInputViewModel(DorogaRepository repo)
    {
        _repo = repo;

        var now = DateTime.Now;
        int dekada = (now.Day - 1) / 10 + 1;
        Gmd = $"{now:yy}{now:MM}{dekada}";
    }

    public DorogaInputViewModel() : this(null!) { }

    [RelayCommand]
    private async Task SaveAsync()
    {
        // === Валидация даты документа ===
        if (!TryValidateDataDokumenta(DataDokumenta, out string? dataError))
        {
            StatusMessage = dataError;
            return;
        }

        try
        {
            var model = new Doroga1
            {
                DGmd = Gmd,
                DPach = NullIfEmpty(Pachka),
                DNakl = NullIfEmpty(Nakl),
                DOtpV = NullIfEmpty(Otpravitel),
                DStotp = NullIfEmpty(StanciyaOtpravleniya),
                DStnazn = NullIfEmpty(StanciyaNaznacheniya),
                DGruz = NullIfEmpty(Gruz),
                DPsila = ParseDecimal(PodyomnayaSila),
                DPoluhat = NullIfEmpty(Poluchatel),
                DVes = ParseDecimal(ChisloMest),
                DData = NullIfEmpty(DataDokumenta),

                DVag1 = NullIfEmpty(Vagon1),
                DVag2 = NullIfEmpty(Vagon2),
                DVag3 = NullIfEmpty(Vagon3),
                DVag4 = NullIfEmpty(Vagon4),

                DDataVv = DateTime.Today,
                DTimeVv = DateTime.Now.ToString("HH:mm:ss")
            };

            await _repo.InsertAsync(model);

            StatusMessage = "Сохранено";
            SaveSucceeded?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка: " + ex.Message;
        }
    }

    [RelayCommand]
    private void Cancel() => SaveSucceeded?.Invoke();

    // ===== Валидация даты ДДММ =====
    private static bool TryValidateDataDokumenta(string input, out string? error)
    {
        error = null;

        // Пустое поле — допустимо
        if (string.IsNullOrWhiteSpace(input))
            return true;

        input = input.Trim();

        // Должно быть ровно 4 цифры
        if (input.Length != 4 || !int.TryParse(input, out _))
        {
            error = "Дата документа должна быть в формате ДДММ (4 цифры)";
            return false;
        }

        int day = int.Parse(input.Substring(0, 2));
        int month = int.Parse(input.Substring(2, 2));

        if (month < 1 || month > 12)
        {
            error = "Месяц должен быть от 01 до 12";
            return false;
        }

        // Используем текущий год для проверки, что день валиден в этом месяце.
        // (Например, 29.02 — только в високосный год.)
        int year = DateTime.Today.Year;

        if (day < 1 || day > DateTime.DaysInMonth(year, month))
        {
            error = $"В месяце {month:00} нет дня {day:00}";
            return false;
        }

        return true;
    }

    // ===== Хелперы =====
    private static string? NullIfEmpty(string? s)
        => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static decimal? ParseDecimal(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        s = s.Replace(',', '.');
        return decimal.TryParse(s,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var v) ? v : null;
    }
}