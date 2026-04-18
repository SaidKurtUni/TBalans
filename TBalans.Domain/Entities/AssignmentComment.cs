using System;

namespace TBalans.Domain.Entities;

/// <summary>
/// Grup ödevlerindeki ortak tartışma ve kaynak paylaşım notlarını (yorumları) temsil eden varlık.
/// Clean Architecture ve OOP prensiplerine uygun tasarlanmıştır.
/// </summary>
public class AssignmentComment
{
    /// <summary>Yorumun benzersiz kimliği</summary>
    public Guid Id { get; private set; }

    /// <summary>Yorumun ait olduğu grup görevinin kimliği (FK)</summary>
    public Guid GroupAssignmentId { get; private set; }

    /// <summary>Yorumu yazan kullanıcının kimliği (FK)</summary>
    public Guid UserId { get; private set; }

    /// <summary>Yorumu yazan kullanıcının Adı Soyadı (Hızlı gösterim için denormalize edilmiş veri)</summary>
    public string UserName { get; private set; } = default!;

    /// <summary>Yorum / Not içeriği</summary>
    public string Content { get; private set; } = default!;

    /// <summary>Eklenen dosyanın sunucudaki URL'si (PDF, Word, Görsel vb.)</summary>
    public string? FileUrl { get; private set; }

    /// <summary>Eklenen dosyanın orijinal adı (Arayüz'de gösterim için)</summary>
    public string? FileName { get; private set; }

    /// <summary>Yorumun oluşturulma tarihi</summary>
    public DateTime CreatedAt { get; private set; }

    // --- Navigation Properties ---

    /// <summary>Ait olduğu grup görevi</summary>
    public virtual GroupAssignment GroupAssignment { get; private set; } = default!;

    /// <summary>Yorumu yazan kullanıcı</summary>
    public virtual User User { get; private set; } = default!;

    // Parametresiz kurucu (ORM araçları için)
    protected AssignmentComment() { }

    public AssignmentComment(
        Guid groupAssignmentId,
        Guid userId,
        string userName,
        string content)
    {
        if (groupAssignmentId == Guid.Empty)
            throw new ArgumentException("Grup görevi kimliği boş olamaz.", nameof(groupAssignmentId));
        if (userId == Guid.Empty)
            throw new ArgumentException("Kullanıcı kimliği boş olamaz.", nameof(userId));
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Kullanıcı adı boş olamaz.", nameof(userName));
        // İş Kuralı: İçerik boşsa bile dosya ekliyse yorum geçerlidir
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Yorum içeriği boş olamaz.", nameof(content));

        Id                = Guid.NewGuid();
        GroupAssignmentId = groupAssignmentId;
        UserId            = userId;
        UserName          = userName;
        Content           = content;
        CreatedAt         = DateTime.UtcNow;
    }

    /// <summary>
    /// Yoruma eklenen dosyanın URL ve adını atar.
    /// </summary>
    public void SetFile(string fileUrl, string fileName)
    {
        FileUrl  = fileUrl;
        FileName = fileName;
    }
}
