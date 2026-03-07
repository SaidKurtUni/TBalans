using System;

namespace TBalans.Application.DTOs;

/// <summary>
/// Takvim arayüzü (Front-End / UI) için gerekli olan görev verilerini taşıyan DTO nesnesi.
/// </summary>
public class CalendarEventDto
{
    /// <summary>
    /// Olayın eşsiz kimliği (Assignment Id'si ile eşleşir)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Takvimde gösterilecek görev başlığı
    /// </summary>
    public string Title { get; set; } = default!;

    /// <summary>
    /// Takvim etkinliğinin başlangıç tarihi
    /// (Son teslim tarihinden 'Tahmini Süre' kadar önce başlar)
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Takvim etkinliğinin bitiş tarihi (Bu genellikle DueDate'tir)
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Olayın kritiklik duruma göre alacağı renk kodu (Örn: Kritikse Kırmızı #FF0000)
    /// </summary>
    public string ColorCode { get; set; } = default!;

    /// <summary>
    /// Etkinliğin kritik seviyede olup olmadığını belirtir
    /// </summary>
    public bool IsCritical { get; set; }
}
