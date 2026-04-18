using System;
using TBalans.Domain.Enums;

namespace TBalans.Domain.Entities;

/// <summary>
/// Bir grubun akademik görevini (ödev, proje, sınav) temsil eden varlık.
/// Clean Architecture ve OOP prensiplerine uygun tasarlanmıştır.
/// </summary>
public class GroupAssignment
{
    /// <summary>Kaydın benzersiz kimliği</summary>
    public Guid Id { get; private set; }

    /// <summary>Bu görevin ait olduğu grubun kimliği (FK)</summary>
    public Guid GroupId { get; private set; }

    /// <summary>Görevi oluşturan üyenin kimliği (FK)</summary>
    public Guid CreatedById { get; private set; }

    /// <summary>Görevin başlığı (Örn: "Hafta 5 Ödevi")</summary>
    public string Title { get; private set; } = default!;

    /// <summary>İlgili ders adı (Örn: "Veri Yapıları")</summary>
    public string CourseName { get; private set; } = default!;

    /// <summary>Görev türü: Ödev, Proje veya Sınav</summary>
    public GroupAssignmentType Type { get; private set; }

    /// <summary>Son teslim tarihi veya sınav tarihi</summary>
    public DateTime DueDate { get; private set; }

    /// <summary>Tahmini bitirme süresi (saat cinsinden)</summary>
    public double? EstimatedHours { get; private set; }

    /// <summary>
    /// Kullanıcının kişisel çalışma notları (Kaynaklar, çözüm süreçleri vb.)
    /// </summary>
    public string? StudentNotes { get; private set; }

    /// <summary>Görev hakkında ek açıklama (isteğe bağlı)</summary>
    public string? Description { get; private set; }

    /// <summary>Kaydın oluşturulma tarihi</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Değerlendirme panosu için sıkça sorulan sorular</summary>
    public string? FaqData { get; private set; }

    /// <summary>Değerlendirme panosu için önemli notlar</summary>
    public string? ImportantNotes { get; private set; }

    // --- Navigation Properties ---

    /// <summary>Ait olduğu grup (Navigation Property)</summary>
    public virtual Group Group { get; private set; } = default!;

    /// <summary>Görevi oluşturan kullanıcı (Navigation Property)</summary>
    public virtual User CreatedBy { get; private set; } = default!;

    /// <summary>Göreve ait tartışma notları / yorumları</summary>
    public virtual ICollection<AssignmentComment>    Comments    { get; private set; } = new List<AssignmentComment>();

    /// <summary>Bu görevi tamamlayan üyeler</summary>
    public virtual ICollection<AssignmentCompletion> Completions { get; private set; } = new List<AssignmentCompletion>();

    private readonly List<GroupAssignmentSubmission> _submissions = new();

    /// <summary>Bu grup görevine yapılan detaylı değerlendirme (teslim) kayıtları</summary>
    public virtual IReadOnlyCollection<GroupAssignmentSubmission> Submissions => _submissions.AsReadOnly();

    // Parametresiz kurucu (ORM araçları için)
    protected GroupAssignment() { }

    public GroupAssignment(
        Guid groupId,
        Guid createdById,
        string title,
        string courseName,
        GroupAssignmentType type,
        DateTime dueDate,
        string? description = null)
    {
        if (groupId == Guid.Empty)
            throw new ArgumentException("Grup kimliği boş olamaz.", nameof(groupId));
        if (createdById == Guid.Empty)
            throw new ArgumentException("Oluşturan kullanıcı kimliği boş olamaz.", nameof(createdById));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Görev başlığı boş olamaz.", nameof(title));
        if (string.IsNullOrWhiteSpace(courseName))
            throw new ArgumentException("Ders adı boş olamaz.", nameof(courseName));
        if (dueDate <= DateTime.UtcNow)
            throw new ArgumentException("Teslim tarihi geçmiş bir tarih olamaz.", nameof(dueDate));

        Id          = Guid.NewGuid();
        GroupId     = groupId;
        CreatedById = createdById;
        Title       = title;
        CourseName  = courseName;
        Type        = type;
        DueDate     = dueDate;
        Description = description;
        EstimatedHours = null;
        StudentNotes   = null;
        CreatedAt   = DateTime.UtcNow;
    }

    /// <summary>
    /// Görevin kritik olup olmadığını belirler.
    /// İş Kuralı: 24 saatten az kaldıysa kritik sayılır.
    /// </summary>
    public bool IsCritical() => (DueDate - DateTime.UtcNow).TotalHours <= 24 && DueDate > DateTime.UtcNow;

    /// <summary>
    /// Görev açıklamasını günceller (Moderatör veya Admin yapabilir — Controller seviyesinde kontrol edilir).
    /// </summary>
    public void UpdateDescription(string? description) => Description = description;

    /// <summary>
    /// Kullanıcı notlarını ve tahmini süreyi günceller.
    /// Bu alan tüm üyeler tarafından güncellenebilir (kişisel çalışma notu).
    /// </summary>
    public void UpdateNotes(string? notes, double? estimatedHours)
    {
        StudentNotes   = notes;
        EstimatedHours = estimatedHours;
    }

    /// <summary>
    /// Değerlendirme panosu için sıkça sorulan soruları ve önemli notları günceller.
    /// </summary>
    public void UpdateFaqAndNotes(string? faqData, string? importantNotes)
    {
        FaqData = faqData;
        ImportantNotes = importantNotes;
    }
}
