using System;
using System.Collections.Generic;
using TBalans.Domain.Enums;

namespace TBalans.Domain.Entities;

/// <summary>
/// Ortak çalışma gruplarını temsil eden varlık.
/// Clean Architecture ve SOLID prensiplerine uygun tasarlanmıştır.
/// </summary>
public class Group
{
    /// <summary>Grubun benzersiz kimliği</summary>
    public Guid Id { get; private set; }

    /// <summary>Grubun adı</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Grubun kısa açıklaması</summary>
    public string? Description { get; private set; }

    /// <summary>Grubun teması veya ikon kodu (emoji veya renk kodu)</summary>
    public string? Theme { get; private set; }

    /// <summary>Gruba katılmak için kullanılacak rastgele üretilmiş 6 haneli davet kodu</summary>
    public string? InviteCode { get; private set; }

    /// <summary>Grubun vize haftası başlangıç tarihi (Akademik Takvim)</summary>
    public DateTime? MidtermWeekStartDate { get; private set; }

    /// <summary>Grubun vize haftası bitiş tarihi (Akademik Takvim)</summary>
    public DateTime? MidtermWeekEndDate { get; private set; }

    /// <summary>Grubun final haftası başlangıç tarihi (Akademik Takvim)</summary>
    public DateTime? FinalWeekStartDate { get; private set; }

    /// <summary>Grubun final haftası bitiş tarihi (Akademik Takvim)</summary>
    public DateTime? FinalWeekEndDate { get; private set; }

    /// <summary>Dönem/Sertifika bitiş tarihi. Bu tarihten sonra ders render edilmez.</summary>
    public DateTime? SemesterEndDate { get; private set; }

    /// <summary>Bütünleme sınavları başlangıç tarihi (Akademik Takvim)</summary>
    public DateTime? MakeupExamsStartDate { get; private set; }

    /// <summary>Bütünleme sınavları bitiş tarihi (Akademik Takvim)</summary>
    public DateTime? MakeupExamsEndDate { get; private set; }

    /// <summary>Grubun gizlilik seviyesi (Public, Request, Private)</summary>
    public GroupPrivacy Privacy { get; private set; }

    /// <summary>Grubu oluşturan kullanıcının kimliği (Admin/Kurucu)</summary>
    public Guid CreatedById { get; private set; }

    /// <summary>Grubun oluşturulma tarihi</summary>
    public DateTime CreatedAt { get; private set; }

    // --- Navigation Properties ---

    /// <summary>Grubu oluşturan kullanıcı (Navigation Property)</summary>
    public virtual User CreatedBy { get; private set; } = default!;

    /// <summary>Grubun tüm üyeleri (One-to-Many)</summary>
    public virtual ICollection<GroupMember> Members { get; private set; } = new List<GroupMember>();

    // Parametresiz kurucu metot (ORM araçları için)
    protected Group() { }

    public Group(
        Guid createdById,
        string name,
        string? description = null,
        string? theme = null,
        GroupPrivacy privacy = GroupPrivacy.Public)
    {
        if (createdById == Guid.Empty)
            throw new ArgumentException("Grubu oluşturan kullanıcı kimliği boş olamaz.", nameof(createdById));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Grup adı boş olamaz.", nameof(name));

        Id          = Guid.NewGuid();
        CreatedById = createdById;
        Name        = name;
        Description = description;
        Theme       = theme;
        Privacy     = privacy;
        CreatedAt   = DateTime.UtcNow;
    }

    /// <summary>
    /// Gruba ilk kurucuyu Admin olarak ekler.
    /// İş Kuralı: Grubu oluşturan kişi otomatik olarak Admin rolünü alır.
    /// </summary>
    public GroupMember AddFounder()
    {
        var member = new GroupMember(Id, CreatedById, GroupRole.Admin, isApproved: true);
        Members.Add(member);
        return member;
    }

    /// <summary>
    /// Gruba yeni bir üye ekler veya katılma isteği oluşturur.
    /// İş Kuralı: Public gruplarda isApproved=true, Request/Private gruplarda isApproved=false ile başlar.
    /// </summary>
    public GroupMember AddMember(Guid userId, GroupRole role = GroupRole.Member)
    {
        bool autoApprove = Privacy == GroupPrivacy.Public;
        var member = new GroupMember(Id, userId, role, isApproved: autoApprove);
        Members.Add(member);
        return member;
    }

    /// <summary>
    /// Grup için rasgele davet kodunu atar.
    /// </summary>
    public void SetInviteCode(string code)
    {
        InviteCode = code;
    }

    /// <summary>
    /// Grubun akademik takvim tarihlerini günceller.
    /// </summary>
    public void UpdateExamWeeks(
        DateTime? midtermStart, DateTime? midtermEnd,
        DateTime? finalStart,   DateTime? finalEnd,
        DateTime? semesterEnd,
        DateTime? makeupStart,  DateTime? makeupEnd)
    {
        MidtermWeekStartDate = midtermStart;
        MidtermWeekEndDate   = midtermEnd;
        FinalWeekStartDate   = finalStart;
        FinalWeekEndDate     = finalEnd;
        SemesterEndDate      = semesterEnd;
        MakeupExamsStartDate = makeupStart;
        MakeupExamsEndDate   = makeupEnd;
    }
}
