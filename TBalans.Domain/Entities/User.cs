using System;
using TBalans.Domain.Enums;

namespace TBalans.Domain.Entities;

/// <summary>
/// Sistemdeki kullanıcıları temsil eden varlık sınıfı.
/// Clean Architecture ve OOP prensiplerine uygun olarak tasarlanmıştır.
/// </summary>
public class User
{
    /// <summary>
    /// Kullanıcının benzersiz kimliği
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Kullanıcının profil fotoğrafı bağlantısı
    /// </summary>
    public string PhotoUrl { get; private set; } = default!;

    /// <summary>
    /// Kullanıcının avatar tercihi (Harf, İkon veya Özel resim)
    /// </summary>
    public AvatarType AvatarType { get; private set; }

    /// <summary>
    /// Kullanıcının kayıtlı olduğu üniversite
    /// </summary>
    public string University { get; private set; } = default!;

    /// <summary>
    /// Kullanıcının bölümü
    /// </summary>
    public string Department { get; private set; } = default!;

    /// <summary>
    /// Kullanıcının sistemdeki itibarı (Rozet kilitlerini açmak için)
    /// </summary>
    public int KarmaPoints { get; private set; }

    /// <summary>
    /// Üniversite mirası (Arşivleme) için akademik yıl (Örn: "2025-2026")
    /// </summary>
    public string AcademicYear { get; private set; } = default!;

    /// <summary>
    /// Üniversite mirası (Arşivleme) için eğitim dönemi (Güz/Bahar)
    /// </summary>
    public Semester Semester { get; private set; }

    // Parametresiz kurucu metot (ORM araçları için)
    protected User() { }

    public User(
        string photoUrl,
        AvatarType avatarType,
        string university = "Bandırma 17 Eylül",
        string department = "Yazılım Mühendisliği",
        string academicYear = "",
        Semester semester = Semester.Fall)
    {
        Id = Guid.NewGuid();
        PhotoUrl = photoUrl;
        AvatarType = avatarType;
        University = string.IsNullOrWhiteSpace(university) ? "Bandırma 17 Eylül" : university;
        Department = string.IsNullOrWhiteSpace(department) ? "Yazılım Mühendisliği" : department;
        KarmaPoints = 0; // Yeni kayıt olan kullanıcının karması sıfırdan başlar
        AcademicYear = academicYear;
        Semester = semester;
    }

    /// <summary>
    /// Kullanıcının karma puanını günceller.
    /// İş kuralı: Karma puanı asla negatif olamaz.
    /// </summary>
    /// <param name="pointsToAdd">Eklenecek veya çıkarılacak puan (negatif değer alabilir)</param>
    public void UpdateKarma(int pointsToAdd)
    {
        KarmaPoints += pointsToAdd;
        if (KarmaPoints < 0)
            KarmaPoints = 0;
    }
}
