using System;

namespace Soft_Givz.Models;

public class Doroga1
{
    // === Поля, вводимые оператором ===
    public string? DGmd { get; set; }
    public string? DPach { get; set; }
    public string? DNakl { get; set; }
    public string? DOtpV { get; set; }
    public string? DStotp { get; set; }
    public string? DStnazn { get; set; }
    public string? DGruz { get; set; }
    public decimal? DPsila { get; set; }     // подъёмная сила
    public decimal? DVes { get; set; }       // ВЕС ← вот оно
    public string? DPoluhat { get; set; }
    public string? DData { get; set; }

    // === Номера вагонов ===
    public string? DVag1 { get; set; }
    public string? DVag2 { get; set; }
    public string? DVag3 { get; set; }
    public string? DVag4 { get; set; }

    // === Служебные ===
    public DateTime? DDataVv { get; set; }
    public string? DTimeVv { get; set; }
}