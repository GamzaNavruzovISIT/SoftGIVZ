using System;

namespace Soft_Givz.Models;

public class Vedom1
{
    // === Поля, вводимые оператором ===
    public string? VGmd { get; set; }
    public string? VPach { get; set; }
    public string? VStan { get; set; }
    public string? VNved { get; set; }
    public string? VTiploc { get; set; }
    public string? VOtp { get; set; }
    public string? VUchastok { get; set; }
    public string? VGruz { get; set; }

    // === Типы вагонов: количество + сокращение (4 шт.) ===
    public int? VKvag1 { get; set; }
    public string? VSokr1 { get; set; }
    public int? VKvag2 { get; set; }
    public string? VSokr2 { get; set; }
    public int? VKvag3 { get; set; }
    public string? VSokr3 { get; set; }
    public int? VKvag4 { get; set; }
    public string? VSokr4 { get; set; }

    // === Время и дата ===
    public decimal? VProstoi { get; set; }
    public decimal? VTrab { get; set; }
    public string? VData { get; set; }

    // === Служебные (заполняются программой) ===
    public DateTime? VDataVv { get; set; }
    public string? VTimeVv { get; set; }
}