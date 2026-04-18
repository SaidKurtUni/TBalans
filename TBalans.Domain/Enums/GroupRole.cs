namespace TBalans.Domain.Enums;

/// <summary>
/// Grup içindeki kullanıcının rolünü tanımlayan enum.
/// RBAC (Role-Based Access Control) kurallarına uygun tasarlanmıştır.
/// </summary>
public enum GroupRole
{
    /// <summary>Sadece okuma, materyal ekleme ve "Tamamladım" işaretleme yetkisi</summary>
    Member = 0,

    /// <summary>Görev ekleme/düzenleme yetkisi; Audit Log tutulur</summary>
    Moderator = 1,

    /// <summary>Tüm yetkiler — grubu oluşturan kişidir</summary>
    Admin = 2
}
