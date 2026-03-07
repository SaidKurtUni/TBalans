using System;
using System.Threading.Tasks;
using TBalans.Domain.Entities;

namespace TBalans.Application.Services;

public partial interface IScheduleService
{
    // Daha önce tanımlanan GetWeeklyScheduleAsync metodu IScheduleService.cs içerisinde mevcut.
    // Ancak dışarıdan Programlama/Takvim ekleyebilmemiz için AddScheduleAsync metoduna da ihtiyacımız var.
    Task<Schedule> AddScheduleAsync(Schedule schedule);
}
