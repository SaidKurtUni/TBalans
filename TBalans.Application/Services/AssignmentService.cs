using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TBalans.Domain.Entities;
using TBalans.Infrastructure;

namespace TBalans.Application.Services;

/// <summary>
/// IAssignmentService arayüzünün (interface) implementasyonu. 
/// Veritabanı ve iş kurallarını (business rules) yönetir.
/// </summary>
public class AssignmentService : IAssignmentService
{
    private readonly TBalansDbContext _context;

    public AssignmentService(TBalansDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public bool IsAssignmentCritical(Assignment assignment)
    {
        if (assignment == null)
            throw new ArgumentNullException(nameof(assignment));

        // Kural: Şu anki zaman + Tahmini Süre > Teslim Tarihi ise ödevi 'Kritik' olarak işaretle
        var estimatedCompletionTime = DateTime.UtcNow.AddHours(assignment.EstimatedHours);
        
        return estimatedCompletionTime > assignment.DueDate;
    }

    public async Task<Assignment> AddAssignmentAsync(Assignment newAssignment)
    {
        if (newAssignment == null)
            throw new ArgumentNullException(nameof(newAssignment));

        // Hata Kontrolü (Conflict Check): Aynı tarih ve saatte (Tamamen aynı DueDate) başka ödev varsa uyarı ver.
        bool hasConflict = await _context.Assignments
            .AnyAsync(a => a.DueDate == newAssignment.DueDate);

        if (hasConflict)
        {
            throw new InvalidOperationException($"Çakışma Hatası: {newAssignment.DueDate:dd.MM.yyyy HH:mm} tarihinde teslim edilmesi gereken başka bir görev zaten var! Lütfen teslim tarihini değiştirin.");
        }

        // Çakışma yoksa veritabanına ekle
        await _context.Assignments.AddAsync(newAssignment);
        await _context.SaveChangesAsync();

        return newAssignment;
    }
}
