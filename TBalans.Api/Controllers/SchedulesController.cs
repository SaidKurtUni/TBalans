using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TBalans.Application.Services;
using TBalans.Domain.Entities;

namespace TBalans.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public SchedulesController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService ?? throw new ArgumentNullException(nameof(scheduleService));
    }

    /// <summary>
    /// Sisteme yeni bir ders veya sınav ekler (Esnek takvim destekli)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateScheduleRequest request)
    {
        if (request == null)
            return BadRequest("Zamanlama veya takvim bilgisi eksik.");

        try
        {
            // İleride UserId token'dan (JWT Claim) alınacak. 
            // Şimdilik test amaçlı sabit bir Guid (veya dışardan gelen bir değer) kullanıyoruz 
            // -Gerçek projede request'ten UserId almak güvenlik açığıdır-
            var userId = request.UserId != Guid.Empty ? request.UserId : Guid.NewGuid();

            var schedule = new Schedule(
                title: request.Title,
                description: request.Description,
                startTime: request.StartTime,
                endTime: request.EndTime,
                dayOfWeek: request.DayOfWeek,
                isExam: request.IsExam,
                location: request.Location,
                userId: userId,
                effectiveStartDate: request.EffectiveStartDate,
                effectiveEndDate: request.EffectiveEndDate,
                occursOnSpecificWeeks: request.OccursOnSpecificWeeks
            );

            var createdSchedule = await _scheduleService.AddScheduleAsync(schedule);

            return Ok(createdSchedule);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Sistemde yapılandırma hatası oluştu.", Details = ex.Message });
        }
    }

    /// <summary>
    /// Vize haftası filtreleme mantığına sahip haftalık programı getirir
    /// Eğer filtrelenen aralıkta Sınav varsa; normal dersler gizlenir.
    /// </summary>
    /// <param name="userId">Kullanıcı kimliği</param>
    /// <param name="date">Haftanın başlangıç veya aranacak spesifik tarihi</param>
    /// <returns>Filtrelenmiş Program</returns>
    [HttpGet("weekly/{date}")]
    public async Task<IActionResult> GetWeeklySchedule([FromQuery] Guid userId, DateTime date)
    {
        if (userId == Guid.Empty)
            return BadRequest("Kullanıcı kimliği (UserId) zorunludur.");

        try
        {
            // Hafta başlangıcını hesaplama garantisi (Opsiyonel, Frontend de Pzt gönderebilir)
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime weekStart = date.AddDays(-1 * diff).Date;
            
            var scheduleList = await _scheduleService.GetWeeklyScheduleAsync(userId, weekStart);

            return Ok(scheduleList);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Haftalık takvim çekilirken hata oluştu.", Details = ex.Message });
        }
    }
}

// Yeni ders/sınav eklenirken API dışarısından alınacak JSON formatı
public class CreateScheduleRequest
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsExam { get; set; }
    public string Location { get; set; } = default!;
    
    public DateTime? EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
    public string? OccursOnSpecificWeeks { get; set; }

    // Test amaçlı eklendi - JWT'de kaldırılacak
    public Guid UserId { get; set; } 
}
