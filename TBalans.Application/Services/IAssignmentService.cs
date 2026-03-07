using System;
using System.Threading.Tasks;
using TBalans.Domain.Entities;

namespace TBalans.Application.Services;

/// <summary>
/// Görev (Ödev/Proje vb.) işlemleri için gerekli olan iş kurallarını barındıran servis arayüzü.
/// </summary>
public interface IAssignmentService
{
    /// <summary>
    /// Verilen ödevin kritik durumda olup olmadığını hesaplar.
    /// Kural: Eğer 'Şu anki zaman + Tahmini Süre > Teslim Tarihi' ise görev kritik sayılır.
    /// </summary>
    /// <param name="assignment">Kritikliği kontrol edilecek görev nesnesi</param>
    /// <returns>Görev kritikse true, değilse false</returns>
    bool IsAssignmentCritical(Assignment assignment);

    /// <summary>
    /// Yeni bir görev eklerken çakışma (Conflict) kontrolü yapar.
    /// Aynı tarih ve saatte başka bir ödev varsa hata fırlatılır.
    /// </summary>
    /// <param name="newAssignment">Eklenecek yeni görev nesnesi</param>
    /// <returns>İşlem başarılıysa eklenen görevi döner</returns>
    Task<Assignment> AddAssignmentAsync(Assignment newAssignment);
    /// <summary>
    /// Verilen tarih aralığındaki görevleri (Assignment) takvim formatında (CalendarEventDto) getirir.
    /// Başlangıç tarihi hesaplanırken EstimatedHours değeri kullanılır.
    /// </summary>
    /// <param name="start">Filtreleme için başlangıç tarihi</param>
    /// <param name="end">Filtreleme için bitiş tarihi</param>
    /// <returns>Takvim olayları listesi</returns>
    Task<System.Collections.Generic.List<DTOs.CalendarEventDto>> GetCalendarEventsAsync(DateTime start, DateTime end);
}
