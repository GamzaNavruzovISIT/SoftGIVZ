using System;

namespace Soft_Givz.Models;

public class Vagon
{
    public string VagTip { get; set; } = "";
    public string? VagNot { get; set; }
    public string? VagNdo { get; set; }
    public decimal? VagPsila { get; set; }
    public string? VagReft { get; set; }
    public decimal? VagCena { get; set; }
    public string? VagName { get; set; }
    public string? VagSokr { get; set; }
    public decimal? VagTar { get; set; }
    public decimal? VagCS { get; set; }
    public decimal? VagTS { get; set; }
    public string? Kom { get; set; }
    public string? TarifGrup { get; set; }
    public DateTime? DataIzm { get; set; }

    // Для отображения в ComboBox. Если сокращение и имя пустые — пустая строка.
    public string Display
    {
        get
        {
            if (string.IsNullOrWhiteSpace(VagSokr) && string.IsNullOrWhiteSpace(VagName))
                return "";

            if (string.IsNullOrWhiteSpace(VagSokr))
                return VagName ?? "";

            if (string.IsNullOrWhiteSpace(VagName))
                return VagSokr;

            return $"{VagSokr} — {VagName}";
        }
    }

    // Признак «пустого» элемента (для валидации)
    public bool IsEmpty => string.IsNullOrWhiteSpace(VagTip);
}