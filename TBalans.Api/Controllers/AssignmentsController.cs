using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TBalans.Application.Services;
using TBalans.Domain.Entities;
using TBalans.Domain.Enums;
using TBalans.Infrastructure;

namespace TBalans.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentsController(
    IAssignmentService assignmentService,
    TBalansDbContext db) : ControllerBase
{
    private readonly IAssignmentService _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
    private readonly TBalansDbContext   _db               = db               ?? throw new ArgumentNullException(nameof(db));

    private Guid GetCurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }

    /// <summary>Yeni bir ödev/görev ekler.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssignmentRequest request)
    {
        if (request == null) return BadRequest("Görev bilgileri geçersiz.");
        try
        {
            var assignment = new Assignment(
                request.UserId, request.Title, request.Description,
                request.EstimatedHours, request.DueDate,
                request.AcademicYear, request.Semester, request.Priority);
            var added = await _assignmentService.AddAssignmentAsync(assignment);
            return Ok(added);
        }
        catch (InvalidOperationException ex) { return Conflict(new { ex.Message }); }
        catch (ArgumentException ex)         { return BadRequest(new { ex.Message }); }
        catch (Exception ex)                 { return StatusCode(500, new { Message = "Sunucu hatası.", Details = ex.Message }); }
    }

    /// <summary>Takvimde gösterilecek etkinlikleri getirir.</summary>
    [HttpGet("calendar")]
    public async Task<IActionResult> GetCalendar([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        if (start == default || end == default)
            return BadRequest("Lütfen geçerli bir start ve end tarihi gönderin.");
        try
        {
            var events = await _assignmentService.GetCalendarEventsAsync(start, end);
            return Ok(events);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Veriler getirilirken hata oluştu.", Details = ex.Message });
        }
    }

    /// <summary>
    /// Kişisel ödevler + grup akademik görevlerini birleştirir (Akademik Genel Bakış).
    /// GET /api/Assignments/all-academic
    /// </summary>
    [HttpGet("all-academic")]
    [Authorize]
    public async Task<IActionResult> GetAllAcademic()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = new List<AcademicItemDto>();

        // 1) Kişisel ödevler
        var personal = await _db.Assignments
            .Where(a => a.UserId == userId && !a.IsCompleted)
            .ToListAsync();

        result.AddRange(personal.Select(a => new AcademicItemDto
        {
            Id             = a.Id.ToString(),
            Title          = a.Title,
            DueDate        = a.DueDate,
            Description    = a.Description,
            Source         = "personal",
            GroupId        = null,
            GroupName      = null,
            StudentNotes   = a.StudentNotes,
            EstimatedHours = a.EstimatedHours,
            TypeLabel      = "Kişisel Ödev",
            Color          = "blue",
            IsCritical     = a.IsCritical()
        }));

        // 2) Grup akademik görevleri
        var groupIds = await _db.GroupMembers
            .Where(gm => gm.UserId == userId && gm.IsApproved)
            .Select(gm => gm.GroupId)
            .ToListAsync();

        if (groupIds.Any())
        {
            var groupAssignments = await _db.GroupAssignments
                .Where(ga => groupIds.Contains(ga.GroupId))
                .Include(ga => ga.Group)
                .ToListAsync();

            result.AddRange(groupAssignments.Select(ga => new AcademicItemDto
            {
                Id             = ga.Id.ToString(),
                Title          = ga.Title,
                DueDate        = ga.DueDate,
                Description    = ga.Description,
                Source         = "group",
                GroupId        = ga.GroupId.ToString(),
                GroupName      = ga.Group.Name,
                StudentNotes   = ga.StudentNotes,
                EstimatedHours = ga.EstimatedHours,
                TypeLabel      = ga.Type.ToString() switch
                {
                    "Homework" => "Grup Ödevi",
                    "Project"  => "Grup Projesi",
                    "Exam"     => "Sınav",
                    _          => "Grup Görevi"
                },
                Color          = "purple",
                IsCritical     = ga.IsCritical()
            }));
        }

        return Ok(result.OrderBy(r => r.DueDate).ToList());
    }

    /// <summary>
    /// Kişisel ödevin notlarını ve tahmini süresini günceller.
    /// PATCH /api/Assignments/{id}/notes
    /// </summary>
    [HttpPatch("{id}/notes")]
    [Authorize]
    public async Task<IActionResult> UpdateNotes(Guid id, [FromBody] UpdateAssignmentNotesDto request)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (assignment == null)
            return NotFound(new { Message = "Görev bulunamadı veya erişim yetkiniz yok." });

        assignment.UpdateNotes(request.StudentNotes, request.EstimatedHours);
        await _db.SaveChangesAsync();

        return Ok(new { Message = "Notlar başarıyla kaydedildi." });
    }
}

// Birleşik akademik görev DTO'su
public class AcademicItemDto
{
    public string   Id             { get; set; } = default!;
    public string   Title          { get; set; } = default!;
    public DateTime DueDate        { get; set; }
    public string?  Description    { get; set; }
    public string   Source         { get; set; } = default!;   // "personal" | "group"
    public string?  GroupId        { get; set; }
    public string?  GroupName      { get; set; }
    public string?  StudentNotes   { get; set; }
    public double?  EstimatedHours { get; set; }
    public string   TypeLabel      { get; set; } = default!;
    public string   Color          { get; set; } = default!;   // "blue" | "purple"
    public bool     IsCritical     { get; set; }
}

public class UpdateAssignmentNotesDto
{
    public string? StudentNotes { get; set; }
    public double? EstimatedHours { get; set; }
}


// Model Binding DTO
public class CreateAssignmentRequest
{
    public Guid     UserId         { get; set; }
    public string   Title          { get; set; } = default!;
    public string   Description    { get; set; } = default!;
    public double   EstimatedHours { get; set; }
    public DateTime DueDate        { get; set; }
    public string   AcademicYear   { get; set; } = default!;
    public Semester Semester       { get; set; }
    public Priority Priority       { get; set; }
}
