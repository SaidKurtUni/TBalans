using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TBalans.Domain.Entities;
using TBalans.Infrastructure;

namespace TBalans.Application.Services;

public class ScheduleService : IScheduleService
{
    private readonly TBalansDbContext _context;

    public ScheduleService(TBalansDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<Schedule>> GetWeeklyScheduleAsync(Guid userId, DateTime weekStartDate)
    {
        DateTime weekEndDate = weekStartDate.AddDays(7); // O haftanın bitiş tarihi (7 gün sonrası)

        // 1. Kullanıcının o haftaya ait (Effective Dates içine düşen) veya genelde geçerli tüm ders programını çekiyoruz
        var allSchedules = await _context.Schedules
            .Where(s => s.UserId == userId 
                && !s.IsCancelled 
                // Either no effective date OR the week intersects with effective dates
                && (!s.EffectiveStartDate.HasValue || s.EffectiveStartDate <= weekEndDate)
                && (!s.EffectiveEndDate.HasValue || s.EffectiveEndDate >= weekStartDate))
            .ToListAsync();

        // 2. Vize Haftası Kuralı (Business Rule):
        // Eğer filtrelenen programlarda 'IsExam == true' (Sınav) olan BİR kayıt (veya daha fazlası) varsa:
        bool isExamWeek = allSchedules.Any(s => s.IsExam);

        if (isExamWeek)
        {
            // O hafta vize/final haftasıdır. 'IsExam == true' olmayan "Normal Dersleri" gizliyoruz. (Listeden çıkarıyoruz)
            // Böylece sadece sınav saatlerini görür.
            return allSchedules.Where(s => s.IsExam).ToList();
        }

        // Eğer sınav yoksa, kullanıcının o haftaki normal (Esnek) programını dönüyoruz.
        return allSchedules;
    }
}
