using System;

namespace TBalans.Domain.Entities;

/// <summary>
/// Grup üyelerinin bir görev için yaptıkları değerlendirme (teslim) kayıtlarını temsil eder.
/// Clean Architecture ve OOP detaylarına uygun oluşturulmuştur.
/// </summary>
public class GroupAssignmentSubmission
{
    /// <summary>
    /// Değerlendirme/Teslim kaydının benzersiz kimliği.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// İlgili grup görevinin benzersiz kimliği (Foreign Key).
    /// </summary>
    public Guid GroupAssignmentId { get; private set; }

    /// <summary>
    /// Görevi teslim eden öğrencinin/üyenin benzersiz kimliği (Foreign Key).
    /// </summary>
    public Guid StudentId { get; private set; }

    /// <summary>
    /// Üyenin bu görevi tamamlarken izlediği yöntemin detaylı açıklaması.
    /// </summary>
    public string MethodDescription { get; private set; } = default!;

    /// <summary>
    /// Bu görev sırasında kullanılan araçlar, diller veya kütüphaneler.
    /// </summary>
    public string ToolsUsed { get; private set; } = default!;

    /// <summary>
    /// Elde edilen sonucun özeti.
    /// </summary>
    public string ResultSummary { get; private set; } = default!;

    /// <summary>
    /// İsteğe bağlı, teslim edilen dosyanın veya kanıtın URL'si.
    /// </summary>
    public string? FileUrl { get; private set; }
    
    /// <summary>
    /// Teslimin oluşturulma tarihi.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    // Navigation Properties
    public virtual GroupAssignment GroupAssignment { get; private set; } = default!;
    public virtual User Student { get; private set; } = default!;

    // Parametresiz kurucu metot (ORM araçları için)
    protected GroupAssignmentSubmission() { }

    public GroupAssignmentSubmission(
        Guid groupAssignmentId,
        Guid studentId,
        string methodDescription,
        string toolsUsed,
        string resultSummary,
        string? fileUrl = null)
    {
        if (groupAssignmentId == Guid.Empty)
            throw new ArgumentException("Grup görev kimliği boş olamaz.", nameof(groupAssignmentId));

        if (studentId == Guid.Empty)
            throw new ArgumentException("Öğrenci kimliği boş olamaz.", nameof(studentId));

        if (string.IsNullOrWhiteSpace(methodDescription))
            throw new ArgumentException("Yöntem açıklaması boş olamaz.", nameof(methodDescription));

        if (string.IsNullOrWhiteSpace(toolsUsed))
            throw new ArgumentException("Kullanılan araçlar boş olamaz.", nameof(toolsUsed));

        if (string.IsNullOrWhiteSpace(resultSummary))
            throw new ArgumentException("Sonuç özeti boş olamaz.", nameof(resultSummary));

        Id = Guid.NewGuid();
        GroupAssignmentId = groupAssignmentId;
        StudentId = studentId;
        MethodDescription = methodDescription;
        ToolsUsed = toolsUsed;
        ResultSummary = resultSummary;
        FileUrl = fileUrl;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Teslim edilen bilgileri güvenli bir şekilde günceller.
    /// </summary>
    public void UpdateSubmissionInfo(string methodDescription, string toolsUsed, string resultSummary, string? fileUrl)
    {
        if (string.IsNullOrWhiteSpace(methodDescription))
            throw new ArgumentException("Yöntem açıklaması boş olamaz.", nameof(methodDescription));

        if (string.IsNullOrWhiteSpace(toolsUsed))
            throw new ArgumentException("Kullanılan araçlar boş olamaz.", nameof(toolsUsed));

        if (string.IsNullOrWhiteSpace(resultSummary))
            throw new ArgumentException("Sonuç özeti boş olamaz.", nameof(resultSummary));

        MethodDescription = methodDescription;
        ToolsUsed = toolsUsed;
        ResultSummary = resultSummary;
        FileUrl = fileUrl ?? FileUrl;
    }
}
