using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TBalans.Domain.Entities;

namespace TBalans.Application.Services;

/// <summary>
/// Kullanıcının haftalık takvim/ders programı (Schedule) ile ilgili iş kurallarını barındıran servis arayüzü.
/// </summary>
public interface IScheduleService
{
    /// <summary>
    /// Belirtilen kullanıcı için, istenen haftaya ait "Akıllı Esnek Programı" (Flexible Schedule) getirir.
    /// Kural: Eğer o haftada bir sınav (IsExam = true) varsa, normal dersleri gizler veya iptal eder.
    /// </summary>
    /// <param name="userId">Kullanıcının Id'si</param>
    /// <param name="weekStartDate">Hesaplanacak haftanın başlangıç tarihi (Pazartesi vs.)</param>
    /// <returns>Filtrelenmiş ve iş kurallarından geçmiş haftalık program listesi</returns>
    Task<List<Schedule>> GetWeeklyScheduleAsync(Guid userId, DateTime weekStartDate);
}
