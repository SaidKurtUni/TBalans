using System;
using TBalans.Domain.Enums;

namespace TBalans.Domain.Entities;

/// <summary>
/// Bir kullanıcının belirli bir gruptaki üyelik bilgisini ve rolünü temsil eden varlık.
/// RBAC kurallarına (Admin, Moderatör, Üye) uygun tasarlanmıştır.
/// </summary>
public class GroupMember
{
    /// <summary>Üyelik kaydının benzersiz kimliği</summary>
    public Guid Id { get; private set; }

    /// <summary>Üyenin ait olduğu grubun kimliği (FK)</summary>
    public Guid GroupId { get; private set; }

    /// <summary>Üye olan kullanıcının kimliği (FK)</summary>
    public Guid UserId { get; private set; }

    /// <summary>Kullanıcının gruptaki rolü (Admin, Moderator, Member)</summary>
    public GroupRole Role { get; private set; }

    /// <summary>
    /// Üyeliğin onaylanıp onaylanmadığı.
    /// Public gruplarda true, Request/Private gruplarda admin onaylanana kadar false kalır.
    /// </summary>
    public bool IsApproved { get; private set; }

    /// <summary>Üyeliğin oluşturulma veya katılma istek tarihi</summary>
    public DateTime JoinedAt { get; private set; }

    // --- Navigation Properties ---

    /// <summary>Üyenin ait olduğu grup (Navigation Property)</summary>
    public virtual Group Group { get; private set; } = default!;

    /// <summary>Üye olan kullanıcı (Navigation Property)</summary>
    public virtual User User { get; private set; } = default!;

    // Parametresiz kurucu metot (ORM araçları için)
    protected GroupMember() { }

    public GroupMember(
        Guid groupId,
        Guid userId,
        GroupRole role = GroupRole.Member,
        bool isApproved = false)
    {
        if (groupId == Guid.Empty)
            throw new ArgumentException("Grup kimliği boş olamaz.", nameof(groupId));

        if (userId == Guid.Empty)
            throw new ArgumentException("Kullanıcı kimliği boş olamaz.", nameof(userId));

        Id         = Guid.NewGuid();
        GroupId    = groupId;
        UserId     = userId;
        Role       = role;
        IsApproved = isApproved;
        JoinedAt   = DateTime.UtcNow;
    }

    /// <summary>
    /// Katılma isteğini onaylar.
    /// İş Kuralı: Sadece Admin rolündeki kişiler onaylayabilir (Controller seviyesinde kontrol edilir).
    /// </summary>
    public void Approve()
    {
        IsApproved = true;
    }

    /// <summary>
    /// Üyenin rolünü günceller.
    /// İş Kuralı: Sadece Admin rolündeki kişiler rol değiştirebilir.
    /// </summary>
    public void UpdateRole(GroupRole newRole)
    {
        Role = newRole;
    }
}
