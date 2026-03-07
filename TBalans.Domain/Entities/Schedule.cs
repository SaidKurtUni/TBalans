using System;

namespace TBalans.Domain.Entities;

/// <summary>
/// Kullanıcının haftalık ders veya sınav takvimini temsil eden varlık sınıfı.
/// </summary>
public class Schedule
{
    /// <summary>
    /// Takvim olayının benzersiz kimliği
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Ders veya sınav başlığı (Örn. 'Matematik-101')
    /// </summary>
    public string Title { get; private set; } = default!;

    /// <summary>
    /// Dersin veya sınavın açıklaması
    /// </summary>
    public string Description { get; private set; } = default!;

    /// <summary>
    /// Başlangıç zamanı (Saat ve Dakika için genellikle TimeSpan veya DateTime kullanılır)
    /// </summary>
    public DateTime StartTime { get; private set; }

    /// <summary>
    /// Bitiş zamanı
    /// </summary>
    public DateTime EndTime { get; private set; }

    /// <summary>
    /// Etkinliğin haftanın hangi günü olduğu (Tekrar eden ders programları için)
    /// </summary>
    public DayOfWeek DayOfWeek { get; private set; }

    /// <summary>
    /// Etkinliğin sınav olup olmadığını belirtir
    /// </summary>
    public bool IsExam { get; private set; }

    /// <summary>
    /// Derslik veya laboratuvar konumu (Örn. 'A Blok Amfi-2')
    /// </summary>
    public string Location { get; private set; } = default!;

    /// <summary>
    /// Sahiplik: Bu takvimin hangi kullanıcıya ait olduğu (Bağlantı/Foreign Key Id'si)
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Takvim bilgisinin geçerli olmaya başladığı başlangıç tarihi (Dönem başlangıcı vb.)
    /// </summary>
    public DateTime? EffectiveStartDate { get; private set; }

    /// <summary>
    /// Takvim bilgisinin bitiş tarihi (Dönem sonu vb.)
    /// </summary>
    public DateTime? EffectiveEndDate { get; private set; }

    /// <summary>
    /// Dersin ya da sınavın o hafta/gün için iptal edilip edilmediği
    /// </summary>
    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Belirli haftalarda tekrarlanan (örn. "2,4,6,8. haftalar") özel durumlar için virgülle ayrılmış haftalar (opsiyonel)
    /// </summary>
    public string? OccursOnSpecificWeeks { get; private set; }

    // ORM (Entity Framework) için parametresiz kurucu
    protected Schedule() { }

    public Schedule(
        string title,
        string description,
        DateTime startTime,
        DateTime endTime,
        DayOfWeek dayOfWeek,
        bool isExam,
        string location,
        Guid userId,
        DateTime? effectiveStartDate = null,
        DateTime? effectiveEndDate = null,
        string? occursOnSpecificWeeks = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Takvim başlığı boş olamaz.", nameof(title));

        if (endTime <= startTime)
            throw new ArgumentException("Bitiş zamanı işlemden önce veya aynı olamaz.", nameof(endTime));

        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        StartTime = startTime;
        EndTime = endTime;
        DayOfWeek = dayOfWeek;
        IsExam = isExam;
        Location = location;
        UserId = userId;

        EffectiveStartDate = effectiveStartDate;
        EffectiveEndDate = effectiveEndDate;
        OccursOnSpecificWeeks = occursOnSpecificWeeks;
        IsCancelled = false; // Varsayılan olarak iptal edilmemiştir
    }

    /// <summary>
    /// İptal durumunu günceller.
    /// </summary>
    public void SetCancelledStatus(bool isCancelled)
    {
        IsCancelled = isCancelled;
    }
}
