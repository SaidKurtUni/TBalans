namespace TBalans.Domain.Enums;

/// <summary>
/// Grubun gizlilik seviyesini tanımlayan enum.
/// </summary>
public enum GroupPrivacy
{
    /// <summary>Herkes katılabilir</summary>
    Public = 0,

    /// <summary>Katılmak için admin onayı gerekir</summary>
    Request = 1,

    /// <summary>Sadece davet edilen kullanıcılar katılabilir</summary>
    Private = 2
}
