using System;
using System.Threading.Tasks;
using TBalans.Domain.Entities;

namespace TBalans.Application.Services;

public partial class ScheduleService
{
    /// <summary>
    /// Sisteme yeni bir ders veya sınav programı ekler.
    /// </summary>
    public async Task<Schedule> AddScheduleAsync(Schedule schedule)
    {
        if (schedule == null)
            throw new ArgumentNullException(nameof(schedule));

        // İş kuralı: Aynı saatte aynı sınıfta başka bir ders var mı gibi çakışma (Conflict) 
        // kontrolleri buraya eklenebilir. Şimdilik doğrudan ekliyoruz.

        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();

        return schedule;
    }
}
