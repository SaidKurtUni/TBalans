using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TBalans.Application.Services;
using TBalans.Domain.Entities;
using TBalans.Domain.Enums;

namespace TBalans.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public AssignmentsController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
    }

    /// <summary>
    /// Yeni bir ödev/görev ekler.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssignmentRequest request)
    {
        if (request == null)
            return BadRequest("Görev bilgileri geçersiz.");

        try
        {
            var assignment = new Assignment(
                request.Title,
                request.Description,
                request.EstimatedHours,
                request.DueDate,
                request.AcademicYear,
                request.Semester
            );

            // AddAssignmentAsync içinde Conflict (Çakışma) kontrolü yapılıyor
            var addedAssignment = await _assignmentService.AddAssignmentAsync(assignment);

            return Ok(addedAssignment);
        }
        catch (InvalidOperationException ex)
        {
            // Business Rule Hatası (Örn: Çakışma)
            return Conflict(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Sunucu hatası oluştu.", Details = ex.Message });
        }
    }

    /// <summary>
    /// Takvimde gösterilecek etkinlikleri (CalendarEventDto) getirir.
    /// </summary>
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
            return StatusCode(500, new { Message = "Veriler getirilirken bir hata oluştu.", Details = ex.Message });
        }
    }
}

// Yeni görev eklenirken API dışarısından alınacak JSON formatı (Model Binding)
public class CreateAssignmentRequest
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public double EstimatedHours { get; set; }
    public DateTime DueDate { get; set; }
    public string AcademicYear { get; set; } = default!;
    public Semester Semester { get; set; }
}
