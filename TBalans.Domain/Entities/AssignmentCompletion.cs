using System;

namespace TBalans.Domain.Entities;

/// <summary>
/// Bir grup üyesinin belirli bir grup görevini tamamladığını kaydeden junction varlık.
/// GroupAssignment ile User arasında M:N ilişkisini yönetir.
/// </summary>
public class AssignmentCompletion
{
    /// <summary>Kaydın benzersiz kimliği</summary>
    public Guid Id { get; private set; }

    /// <summary>Tamamlanan grup görevinin kimliği (FK)</summary>
    public Guid GroupAssignmentId { get; private set; }

    /// <summary>Görevi tamamlayan üyenin kimliği (FK)</summary>
    public Guid UserId { get; private set; }

    /// <summary>Tamamlanma tarihi (UTC)</summary>
    public DateTime CompletedAt { get; private set; }

    // --- Navigation Properties ---

    /// <summary>Tamamlanan grup görevi</summary>
    public virtual GroupAssignment GroupAssignment { get; private set; } = default!;

    /// <summary>Görevi tamamlayan kullanıcı</summary>
    public virtual User User { get; private set; } = default!;

    // ORM için parametresiz kurucu
    protected AssignmentCompletion() { }

    public AssignmentCompletion(Guid groupAssignmentId, Guid userId)
    {
        if (groupAssignmentId == Guid.Empty)
            throw new ArgumentException("Grup görevi kimliği boş olamaz.", nameof(groupAssignmentId));
        if (userId == Guid.Empty)
            throw new ArgumentException("Kullanıcı kimliği boş olamaz.", nameof(userId));

        Id                = Guid.NewGuid();
        GroupAssignmentId = groupAssignmentId;
        UserId            = userId;
        CompletedAt       = DateTime.UtcNow;
    }
}
