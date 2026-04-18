using System;
using TBalans.Domain.Enums;

namespace TBalans.Domain.Entities;

/// <summary>
/// Ödev, proje, quiz gibi görevleri temsil eden varlık sınıfı.
/// Clean Architecture ve OOP detaylarına uygun oluşturulmuştur.
/// </summary>
public class Assignment
{
    /// <summary>
    /// Görevin benzersiz kimliği
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Bu görevin ait olduğu kullanıcının kimliği (Foreign Key)
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Görevin başlığı
    /// </summary>
    public string Title { get; private set; } = default!;

    /// <summary>
    /// Görevin açıklaması ve detayları
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Görevin tahmini tamamlanma süresi (Saat cinsinden).
    /// </summary>
    public double? EstimatedHours { get; private set; }

    /// <summary>
    /// Kullanıcının kişisel çalışma notları.
    /// </summary>
    public string? StudentNotes { get; private set; }

    /// <summary>
    /// Görevin son teslim tarihi
    /// </summary>
    public DateTime DueDate { get; private set; }

    /// <summary>
    /// Görevin tamamlanıp tamamlanmadığını belirtir
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Görevin öncelik seviyesi
    /// </summary>
    public Priority Priority { get; private set; }

    /// <summary>
    /// Üniversite mirası (Arşivleme) için akademik yıl (Örn: "2025-2026")
    /// </summary>
    public string AcademicYear { get; private set; } = default!;

    /// <summary>
    /// Üniversite mirası (Arşivleme) için eğitim dönemi (Güz/Bahar)
    /// </summary>
    public Semester Semester { get; private set; }

    /// <summary>
    /// Görevin oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Görevin ait olduğu kullanıcı (Navigation Property)
    /// </summary>
    public virtual User User { get; private set; } = default!;

    // Parametresiz kurucu metot (ORM araçları için)
    protected Assignment() { }

    public Assignment(
        Guid userId,
        string title,
        string? description,
        double? estimatedHours,
        DateTime dueDate,
        string academicYear,
        Semester semester,
        Priority priority = Priority.Medium)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Kullanıcı kimliği boş olamaz.", nameof(userId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Görev başlığı boş olamaz.", nameof(title));

        if (estimatedHours.HasValue && estimatedHours <= 0)
            throw new ArgumentException("Tahmini bitirme süresi 0'dan büyük olmalıdır.", nameof(estimatedHours));

        if (dueDate <= DateTime.UtcNow)
            throw new ArgumentException("Son teslim tarihi geçmiş bir zaman olamaz.", nameof(dueDate));

        Id = Guid.NewGuid();
        UserId = userId;
        Title = title;
        Description = description;
        EstimatedHours = estimatedHours;
        StudentNotes = null;
        DueDate = dueDate;
        AcademicYear = academicYear;
        Semester = semester;
        Priority = priority;
        IsCompleted = false;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Görevin tahmini süresi ile son teslim tarihini kıyaslar. 
    /// Eğer kalan süre, tahmini süreden az ise kritik durumu bildirir (Fail-Fast algoritması temeli).
    /// </summary>
    /// <returns>Eğer görev kritik bir durumdaysa true döner.</returns>
    public bool IsCritical()
    {
        if (!EstimatedHours.HasValue) return false;
        var remainingHours = (DueDate - DateTime.UtcNow).TotalHours;
        return remainingHours > 0 && remainingHours <= EstimatedHours.Value * 1.5; // Tahmini sürenin 1.5 katı kadar kalınca kritik say
    }

    /// <summary>
    /// Görevi tamamlanmış olarak işaretler.
    /// </summary>
    public void MarkAsCompleted()
    {
        IsCompleted = true;
    }

    /// <summary>
    /// Kişisel çalışma notlarını ve tahmini süreyi günceller.
    /// </summary>
    public void UpdateNotes(string? notes, double? estimatedHours)
    {
        StudentNotes = notes;
        EstimatedHours = estimatedHours;
    }
}
