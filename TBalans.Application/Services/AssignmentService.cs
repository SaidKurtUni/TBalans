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

    public async System.Threading.Tasks.Task<System.Collections.Generic.List<DTOs.CalendarEventDto>> GetCalendarEventsAsync(DateTime start, DateTime end)
    {
        // İstenen tarih aralığındaki görevleri çekiyoruz
        var assignments = await _context.Assignments
            .Where(a => a.DueDate >= start && a.DueDate <= end)
            .ToListAsync();

        var calendarEvents = new System.Collections.Generic.List<DTOs.CalendarEventDto>();

        foreach (var assignment in assignments)
        {
            bool isCritical = IsAssignmentCritical(assignment);

            // Akıllı Dönüştürücü (Smart Converter): 
            // Görevin başlangıç tarihini Teslim Tarihinden Tahmini Süreyi çıkararak buluyoruz (Geriye dönük hesaplama)
            var startDate = assignment.DueDate.AddHours(-assignment.EstimatedHours);

            // Json tabanlı UI için verileri haritalıyoruz (mapping)
            calendarEvents.Add(new DTOs.CalendarEventDto
            {
                Id = assignment.Id,
                Title = assignment.Title,
                StartDate = startDate,
                EndDate = assignment.DueDate,
                IsCritical = isCritical,
                ColorCode = isCritical ? "#FF4B4B" : "#4B7DFF" // Kritikse Kırmızı, değilse Mavi kod
            });
        }

        return calendarEvents;
    }
}
