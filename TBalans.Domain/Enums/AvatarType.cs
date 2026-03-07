namespace TBalans.Domain.Enums;

/// <summary>
/// Kullanıcının profilinde gösterilecek avatar tipini belirler.
/// </summary>
public enum AvatarType
{
    /// <summary>
    /// İsmin baş harfi
    /// </summary>
    Letter = 1,
    
    /// <summary>
    /// Hazır ikonlar
    /// </summary>
    Icon = 2,
    
    /// <summary>
    /// Kullanıcının yüklediği özel resim
    /// </summary>
    Custom = 3
}
