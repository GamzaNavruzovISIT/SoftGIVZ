using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private readonly VagonRepository _vagonRepo;

    [ObservableProperty] private string _gmd = "";

    [ObservableProperty] private string _pachka = "";
    [ObservableProperty] private string _stanciya = "";
    [ObservableProperty] private string _nomerVedomosti = "";
    [ObservableProperty] private string _tipLokomotiva = "";
    [ObservableProperty] private string _otpravitel = "";
    [ObservableProperty] private string _uchastok = "";
    [ObservableProperty] private string _gruz = "";

    // ===== Вагоны =====
    [ObservableProperty] private string _vagon1 = "";
    [ObservableProperty] private Vagon? _selectedSokr1;

    [ObservableProperty] private string _vagon2 = "";
    [ObservableProperty] private Vagon? _selectedSokr2;

    [ObservableProperty] private string _vagon3 = "";
    [ObservableProperty] private Vagon? _selectedSokr3;

    [ObservableProperty] private string _vagon4 = "";
    [ObservableProperty] private Vagon? _selectedSokr4;

    public ObservableCollection<Vagon> Vagons { get; } = new();

    [ObservableProperty] private string _vremyaProstoya = "";
    [ObservableProperty] private string _vremyaRaboty = "";
    [ObservableProperty] private string _dataDokumenta = "";

    [ObservableProperty] private string _statusMessage = "";

    public event Action? SaveSucceeded;
    public Func<string, Task<bool>>? ConfirmSaveAnyway;

    public VedomInputViewModel(VedomRepository repo, VagonRepository vagonRepo)
    {
        _repo = repo;
        _vagonRepo = vagonRepo;

        var now = DateTime.Now;
        int dekada = (now.Day - 1) / 10 + 1;
        Gmd = $"{now:yy}{now:MM}{dekada}";
    }

    public VedomInputViewModel() : this(null!, null!) { }

    public async Task LoadVagonsAsync()
    {
        try
        {
            var list = await _vagonRepo.GetAllAsync();
            Vagons.Clear();
            foreach (var v in list) Vagons.Add(v);
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка загрузки справочника вагонов: " + ex.Message;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        // === 1. Проверка обязательных полей ===
        var missing = GetMissingRequiredFields();
        if (missing.Count > 0)
        {
            var message =
                "Не заполнены обязательные поля:\n\n• " +
                string.Join("\n• ", missing) +
                "\n\nВсё равно сохранить?";

            bool saveAnyway = ConfirmSaveAnyway != null
                ? await ConfirmSaveAnyway(message)
                : false;

            if (!saveAnyway)
                return;
        }

        // === 2. Дата ДДММ ===
        if (!TryValidateDataDokumenta(DataDokumenta, out string? dataError))
        {
            StatusMessage = dataError ?? "Ошибка в дате документа";
            return;
        }

        // === 3. Вагоны ===
        if (!TryValidateVagons(out string? vagonError))
        {
            StatusMessage = vagonError ?? "Ошибка в вагонах";
            return;
        }

        // === 4. Сохранение ===
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
                VSokr1 = SelectedSokr1?.VagSokr,
                VKvag2 = ParseInt(Vagon2),
                VSokr2 = SelectedSokr2?.VagSokr,
                VKvag3 = ParseInt(Vagon3),
                VSokr3 = SelectedSokr3?.VagSokr,
                VKvag4 = ParseInt(Vagon4),
                VSokr4 = SelectedSokr4?.VagSokr,

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

    // ===== Обязательные поля =====
    private List<string> GetMissingRequiredFields()
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(Gmd)) missing.Add("Год, Месяц, Декада (ГМД)");
        if (string.IsNullOrWhiteSpace(Pachka)) missing.Add("Номер пачки");
        if (string.IsNullOrWhiteSpace(Stanciya)) missing.Add("Станция");
        if (string.IsNullOrWhiteSpace(NomerVedomosti)) missing.Add("№ ведомости");
        if (string.IsNullOrWhiteSpace(TipLokomotiva)) missing.Add("Тип локомотива");
        if (string.IsNullOrWhiteSpace(Otpravitel)) missing.Add("Отправитель");
        if (string.IsNullOrWhiteSpace(Uchastok)) missing.Add("Участок");
        if (string.IsNullOrWhiteSpace(Gruz)) missing.Add("Груз");
        if (string.IsNullOrWhiteSpace(DataDokumenta)) missing.Add("Дата (ДДММ)");   // ← новое

        return missing;
    }

    private bool TryValidateVagons(out string? error)
    {
        error = null;

        var pairs = new (string kol, Vagon? tip, int row)[]
        {
            (Vagon1, SelectedSokr1, 1),
            (Vagon2, SelectedSokr2, 2),
            (Vagon3, SelectedSokr3, 3),
            (Vagon4, SelectedSokr4, 4),
        };

        foreach (var (kol, tip, row) in pairs)
        {
            int? k = ParseInt(kol);
            bool hasKol = k.HasValue && k.Value > 0;
            bool hasTip = tip != null;

            if (hasKol && !hasTip)
            {
                error = $"Строка вагона {row}: указано количество, но не выбран тип";
                return false;
            }

            if (hasTip && !hasKol)
            {
                error = $"Строка вагона {row}: выбран тип, но не указано количество";
                return false;
            }
        }

        return true;
    }

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