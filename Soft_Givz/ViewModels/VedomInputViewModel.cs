using System;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Soft_Givz.Models;
using Soft_Givz.Repositories;

namespace Soft_Givz.ViewModels;

public partial class VedomInputViewModel : ViewModelBase
{
    private readonly VedomRepository _repo;

    [ObservableProperty] private string _gmd = "";

    [ObservableProperty] private string _pachka = "";
    [ObservableProperty] private string _stanciya = "";
    [ObservableProperty] private string _nomerVedomosti = "";
    [ObservableProperty] private string _tipLokomotiva = "";
    [ObservableProperty] private string _otpravitel = "";
    [ObservableProperty] private string _uchastok = "";
    [ObservableProperty] private string _gruz = "";

    [ObservableProperty] private string _vagon1 = "";
    [ObservableProperty] private string _sokr1 = "";
    [ObservableProperty] private string _vagon2 = "";
    [ObservableProperty] private string _sokr2 = "";
    [ObservableProperty] private string _vagon3 = "";
    [ObservableProperty] private string _sokr3 = "";
    [ObservableProperty] private string _vagon4 = "";
    [ObservableProperty] private string _sokr4 = "";

    [ObservableProperty] private string _vremyaProstoya = "";
    [ObservableProperty] private string _vremyaRaboty = "";
    [ObservableProperty] private string _dataDokumenta = "";

    [ObservableProperty] private string _statusMessage = "";

    public event Action? SaveSucceeded;

    public VedomInputViewModel(VedomRepository repo)
    {
        _repo = repo;

        var now = DateTime.Now;
        int dekada = (now.Day - 1) / 10 + 1;
        Gmd = $"{now:yy}{now:MM}{dekada}";
    }

    public VedomInputViewModel() : this(null!) { }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!TryValidateDataDokumenta(DataDokumenta, out string? dataError))
        {
            StatusMessage = dataError;
            return;
        }

        try
        {
            var model = new Vedom1
            {
                VGmd = Gmd,
                VPach = NullIfEmpty(Pachka),
                VStan = NullIfEmpty(Stanciya),
                VNved = NullIfEmpty(NomerVedomosti),
                VTiploc = NullIfEmpty(TipLokomotiva),
                VOtp = NullIfEmpty(Otpravitel),
                VUchastok = NullIfEmpty(Uchastok),
                VGruz = NullIfEmpty(Gruz),

                VKvag1 = ParseInt(Vagon1),
                VSokr1 = NullIfEmpty(Sokr1),
                VKvag2 = ParseInt(Vagon2),
                VSokr2 = NullIfEmpty(Sokr2),
                VKvag3 = ParseInt(Vagon3),
                VSokr3 = NullIfEmpty(Sokr3),
                VKvag4 = ParseInt(Vagon4),
                VSokr4 = NullIfEmpty(Sokr4),

                VProstoi = ParseDecimal(VremyaProstoya),
                VTrab = ParseDecimal(VremyaRaboty),
                VData = NullIfEmpty(DataDokumenta),

                VDataVv = DateTime.Today,
                VTimeVv = DateTime.Now.ToString("HH:mm:ss")
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

    private static bool TryValidateDataDokumenta(string input, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(input)) return true;

        input = input.Trim();
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

        int year = DateTime.Today.Year;
        if (day < 1 || day > DateTime.DaysInMonth(year, month))
        {
            error = $"В месяце {month:00} нет дня {day:00}";
            return false;
        }

        return true;
    }

    private static string? NullIfEmpty(string? s)
        => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static int? ParseInt(string? s)
        => int.TryParse(s?.Trim(), out var v) ? v : null;

    private static decimal? ParseDecimal(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        s = s.Replace(',', '.');
        return decimal.TryParse(s, NumberStyles.Any,
            CultureInfo.InvariantCulture, out var v) ? v : null;
    }
}