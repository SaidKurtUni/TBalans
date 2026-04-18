using System;

namespace TBalans.Domain.Entities;

/// <summary>
/// Öğrencinin ders programındaki rutin etkinlikleri (ders vb.) temsil eden varlık sınıfı.
/// Clean Architecture ve OOP detaylarına uygun oluşturulmuştur.
/// </summary>
public class Schedule
{
    /// <summary>
    /// Program öğesinin benzersiz kimliği
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Bu programın ait olduğu kullanıcının kimliği (Foreign Key)
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Program öğesinin başlığı (Örn: "Yazılım Mühendisliği Dersi")
    /// </summary>
    public string Title { get; private set; } = default!;

    /// <summary>
    /// Haftanın hangi günü olduğu
    /// </summary>
    public DayOfWeek DayOfWeek { get; private set; }

    /// <summary>
    /// Başlangıç saati
    /// </summary>
    public TimeSpan StartTime { get; private set; }

    /// <summary>
    /// Bitiş saati
    /// </summary>
    public TimeSpan EndTime { get; private set; }

    /// <summary>
    /// Program öğesinin ait olduğu kullanıcı (Navigation Property)
    /// </summary>
    public virtual User User { get; private set; } = default!;

    /// <summary>
    /// Program öğesinin iptal edilip edilmediği
    /// </summary>
    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Geçerli başlangıç tarihi (varsa)
    /// </summary>
    public DateTime? EffectiveStartDate { get; private set; }

    /// <summary>
    /// Geçerli bitiş tarihi (varsa)
    /// </summary>
    public DateTime? EffectiveEndDate { get; private set; }

    /// <summary>
    /// Bu program öğesinin bir sınav olup olmadığını belirtir.
    /// </summary>
    public bool IsExam { get; private set; }

    /// <summary>
    /// Sınav veya dersin yapılacağı konum/sınıf bilgisi
    /// </summary>
    public string? Location { get; private set; }

    /// <summary>
    /// Özel bir açıklama (zorunlu değil)
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Belirli haftalarda olup olmadığını belirtir
    /// </summary>
    public string? OccursOnSpecificWeeks { get; private set; }

    // Parametresiz kurucu metot (ORM araçları için)
    protected Schedule() { }

    public Schedule(
        Guid userId,
        string title,
        DayOfWeek dayOfWeek,
        TimeSpan startTime,
        TimeSpan endTime,
        bool isExam = false,
        string? location = null,
        string? description = null,
        DateTime? effectiveStartDate = null,
        DateTime? effectiveEndDate = null,
        string? occursOnSpecificWeeks = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Kullanıcı kimliği boş olamaz.", nameof(userId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Program başlığı boş olamaz.", nameof(title));

        if (startTime >= endTime)
            throw new ArgumentException("Başlangıç saati, bitiş saatinden önce olmalıdır.", nameof(startTime));

        Id = Guid.NewGuid();
        UserId = userId;
        Title = title;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        IsCancelled = false;
        EffectiveStartDate = effectiveStartDate;
        EffectiveEndDate = effectiveEndDate;
        IsExam = isExam;
        Location = location;
        Description = description;
        OccursOnSpecificWeeks = occursOnSpecificWeeks;
    }

    /// <summary>
    /// Program öğesini iptal eder.
    /// </summary>
    public void Cancel()
    {
        IsCancelled = true;
    }
}
