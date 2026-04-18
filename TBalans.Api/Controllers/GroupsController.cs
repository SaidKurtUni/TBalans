using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TBalans.Domain.Entities;
using TBalans.Domain.Enums;
using TBalans.Infrastructure;

namespace TBalans.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Tüm işlemler için JWT kimlik doğrulaması zorunludur
public class GroupsController : ControllerBase
{
    private readonly TBalansDbContext   _db;
    private readonly IWebHostEnvironment _env;

    public GroupsController(TBalansDbContext db, IWebHostEnvironment env)
    {
        _db  = db  ?? throw new ArgumentNullException(nameof(db));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    // --- Yardımcı: Token'daki kullanıcı kimliğini al ---
    private Guid GetCurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }

    /// <summary>
    /// Giriş yapan kullanıcının üye olduğu grupları listeler (sadece onaylanmış üyelikler).
    /// GET /api/Groups
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyGroups()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var groups = await _db.GroupMembers
            .Where(gm => gm.UserId == userId && gm.IsApproved)
            .Include(gm => gm.Group)
                .ThenInclude(g => g.Members)
            .Select(gm => new
            {
                gm.Group.Id,
                gm.Group.Name,
                gm.Group.Description,
                gm.Group.Theme,
                Privacy                = gm.Group.Privacy.ToString(),
                InviteCode             = gm.Group.InviteCode,
                MemberCount            = gm.Group.Members.Count(m => m.IsApproved),
                MyRole                 = gm.Role.ToString(),
                gm.Group.CreatedAt,
                gm.Group.MidtermWeekStartDate,
                gm.Group.MidtermWeekEndDate,
                gm.Group.FinalWeekStartDate,
                gm.Group.FinalWeekEndDate,
                gm.Group.SemesterEndDate,
                gm.Group.MakeupExamsStartDate,
                gm.Group.MakeupExamsEndDate
            })
            .ToListAsync();

        return Ok(groups);
    }

    /// <summary>
    /// Yeni bir grup oluşturur. Kurucuyu otomatik olarak Admin olarak ekler.
    /// POST /api/Groups
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGroupRequest request)
    {
        if (request == null || !ModelState.IsValid)
            return BadRequest("Grup bilgileri geçersiz.");

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try
        {
            var group   = new Group(userId, request.Name, request.Description, request.Theme, request.Privacy);
            var founder = group.AddFounder(); // Kurucu otomatik Admin yapılır

            _db.Groups.Add(group);
            await _db.SaveChangesAsync();

            return Ok(new { group.Id, group.Name, group.Privacy, Message = "Grup başarıyla oluşturuldu." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Grup oluşturulurken bir hata oluştu.", Details = ex.Message });
        }
    }

    /// <summary>
    /// Bir gruba katılma isteği gönderir.
    /// - Public gruplar: Anında onaylanır.
    /// - Request/Private gruplar: Admin onayı bekler.
    /// POST /api/Groups/{groupId}/join
    /// </summary>
    [HttpPost("{groupId:guid}/join")]
    public async Task<IActionResult> JoinGroup(Guid groupId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var group = await _db.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null) return NotFound("Grup bulunamadı.");

        // Mükerrer üyelik kontrolü
        var existing = group.Members.FirstOrDefault(gm => gm.UserId == userId);
        if (existing != null)
        {
            var status = existing.IsApproved ? "zaten üyesiniz" : "katılma isteğiniz beklemede";
            return Conflict(new { Message = $"Bu gruba {status}." });
        }

        if (group.Privacy == GroupPrivacy.Private)
            return Forbid(); // Gizli gruplara sadece davet ile katılınabilir

        try
        {
            var member = group.AddMember(userId); // Privacy'e göre IsApproved otomatik set edilir
            await _db.SaveChangesAsync();

            var msg = member.IsApproved
                ? "Gruba başarıyla katıldınız."
                : "Katılma isteğiniz alındı. Admin onayını bekleyiniz.";

            return Ok(new { member.IsApproved, Message = msg });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Gruba katılınırken bir hata oluştu.", Details = ex.Message });
        }
    }

    /// <summary>
    /// Bir gruptaki üyeleri listeler. Sadece grubun mevcut üyeleri görebilir.
    /// GET /api/Groups/{groupId}/members
    /// </summary>
    [HttpGet("{groupId:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid groupId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        // Sadece grubun üyeleri diğer üyeleri görebilir
        var isMember = await _db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsApproved);

        if (!isMember) return Forbid();

        var members = await _db.GroupMembers
            .Where(gm => gm.GroupId == groupId && gm.IsApproved)
            .Include(gm => gm.User)
            .Select(gm => new
            {
                gm.UserId,
                Email      = gm.User.Email,
                University = gm.User.University,
                Department = gm.User.Department,
                Role       = gm.Role.ToString(),
                gm.JoinedAt
            })
            .ToListAsync();

        return Ok(members);
    }

    /// <summary>
    /// Bekleyen katılma isteklerini onaylar. Sadece Admin yapabilir.
    /// POST /api/Groups/{groupId}/members/{memberId}/approve
    /// </summary>
    [HttpPost("{groupId:guid}/members/{memberId:guid}/approve")]
    public async Task<IActionResult> ApproveMember(Guid groupId, Guid memberId)
    {
        var adminId = GetCurrentUserId();
        if (adminId == Guid.Empty) return Unauthorized();

        // Yetki kontrolü: Sadece Admin onaylayabilir
        var isAdmin = await _db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == adminId && gm.Role == GroupRole.Admin && gm.IsApproved);

        if (!isAdmin) return Forbid();

        var member = await _db.GroupMembers.FirstOrDefaultAsync(gm => gm.Id == memberId && gm.GroupId == groupId);
        if (member == null) return NotFound("Üyelik kaydı bulunamadı.");
        if (member.IsApproved) return Conflict(new { Message = "Bu üyelik zaten onaylı." });

        member.Approve();
        await _db.SaveChangesAsync();

        return Ok(new { Message = "Üyelik onaylandı." });
    }

    /// <summary>
    /// Gruba rastgele 6 haneli davet kodu üretir. Sadece Admin yapabilir.
    /// POST /api/Groups/{groupId}/generate-invite
    /// </summary>
    [HttpPost("{groupId:guid}/generate-invite")]
    public async Task<IActionResult> GenerateInviteCode(Guid groupId)
    {
        var adminId = GetCurrentUserId();
        if (adminId == Guid.Empty) return Unauthorized();

        var isAdmin = await _db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == adminId && gm.Role == GroupRole.Admin && gm.IsApproved);

        if (!isAdmin) return Forbid();

        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null) return NotFound("Grup bulunamadı.");

        string code;
        bool isUnique = false;
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        
        do
        {
            code = new string(Enumerable.Repeat(chars, 6)
              .Select(s => s[random.Next(s.Length)]).ToArray());
            
            isUnique = !await _db.Groups.AnyAsync(g => g.InviteCode == code);
        } while (!isUnique);

        group.SetInviteCode(code);
        await _db.SaveChangesAsync();

        return Ok(new { InviteCode = code, Message = "Davet kodu oluşturuldu." });
    }

    /// <summary>
    /// Davet kodu kullanarak gruba 'Member' olarak anında katılır.
    /// POST /api/Groups/join-by-code
    /// </summary>
    [HttpPost("join-by-code")]
    public async Task<IActionResult> JoinByCode([FromBody] JoinByCodeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Code))
            return BadRequest("Davet kodu gerekli.");

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var group = await _db.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.InviteCode == request.Code);

        if (group == null) return NotFound("Geçersiz davet kodu.");

        // Zaten üye mi?
        var existing = group.Members.FirstOrDefault(gm => gm.UserId == userId);
        if (existing != null)
        {
            if (existing.IsApproved) return Conflict(new { Message = "Bu gruba zaten üyesiniz." });
            
            // Eğer isteği beklemedeyse, kodla geldiği için direkt onayla
            existing.Approve();
        }
        else
        {
             // Yeni member (davet kodu ile gelen direkt onaylıdır, public/private fark etmez)
             var member = new GroupMember(group.Id, userId, GroupRole.Member, isApproved: true);
             _db.GroupMembers.Add(member);
        }

        await _db.SaveChangesAsync();

        return Ok(new { GroupId = group.Id, Message = "Gruba başarıyla katıldınız." });
    }

    /// <summary>
    /// Gruptaki akademik görevleri (ödev/proje/sınav) tarihe göre sıralı listeler.
    /// Sadece grubun onaylı üyeleri görüntüleyebilir.
    /// GET /api/Groups/{groupId}/assignments
    /// </summary>
    [HttpGet("{groupId:guid}/assignments")]
    public async Task<IActionResult> GetGroupAssignments(Guid groupId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        // Sadece üyeler görebilir
        var isMember = await _db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsApproved);
        if (!isMember) return Forbid();

        var assignments = await _db.GroupAssignments
            .Where(ga => ga.GroupId == groupId)
            .Include(ga => ga.CreatedBy)
            .Include(ga => ga.Completions)   // Tamamlama sayısı için
            .OrderBy(ga => ga.DueDate)
            .Select(ga => new
            {
                ga.Id,
                ga.Title,
                ga.CourseName,
                Type              = ga.Type.ToString(),
                ga.DueDate,
                ga.Description,
                ga.EstimatedHours,
                ga.StudentNotes,
                ga.CreatedAt,
                CreatedBy         = ga.CreatedBy.Email,
                IsCritical        = ga.IsCritical(),
                CompletedCount    = ga.Completions.Count,
                IsCompletedByMe   = ga.Completions.Any(c => c.UserId == userId)
            })
            .ToListAsync();

        return Ok(assignments);
    }

    /// <summary>
    /// Tek bir grup görevinin detaylarını, üye listesini ve tamamlama durumlarını getirir.
    /// GET /api/Groups/{groupId}/assignments/{assignmentId}
    /// </summary>
    [HttpGet("{groupId:guid}/assignments/{assignmentId:guid}")]
    public async Task<IActionResult> GetGroupAssignment(Guid groupId, Guid assignmentId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var isMember = await _db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsApproved);
        if (!isMember) return Forbid();

        // Görev + createdBy + yorumlar + tamamlamalar
        var assignment = await _db.GroupAssignments
            .Include(ga => ga.CreatedBy)
            .Include(ga => ga.Comments)
            .Include(ga => ga.Completions)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(ga => ga.Id == assignmentId && ga.GroupId == groupId);

        if (assignment == null) return NotFound("Görev bulunamadı.");

        // Tamamlayan kullanıcı ID'lerini belleğe al (EF Core closure hatasını önler)
        var completedUserIds = assignment.Completions
            .Select(c => c.UserId)
            .ToHashSet();

        // Gruptaki tüm onaylı üyeler
        var members = await _db.GroupMembers
            .Where(gm => gm.GroupId == groupId && gm.IsApproved)
            .Include(gm => gm.User)
            .Select(gm => new
            {
                UserId   = gm.UserId,
                UserName = gm.User.Email,
                Role     = gm.Role.ToString(),
                // HashSet.Contains SQL dışında in-memory çalışır — EF client evaluation
                IsCompleted = completedUserIds.Contains(gm.UserId)
            })
            .ToListAsync();

        var result = new
        {
            assignment.Id,
            assignment.Title,
            assignment.CourseName,
            Type            = assignment.Type.ToString(),
            assignment.DueDate,
            assignment.Description,
            assignment.EstimatedHours,
            assignment.StudentNotes,
            assignment.FaqData,
            assignment.ImportantNotes,
            assignment.CreatedAt,
            CreatedBy       = assignment.CreatedBy.Email,
            IsCritical      = assignment.IsCritical(),
            IsCompletedByMe = completedUserIds.Contains(userId),
            CompletedCount  = completedUserIds.Count,
            TotalMembers    = members.Count,
            Members         = members,
            Comments        = assignment.Comments
                .OrderBy(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id, c.UserName, c.Content,
                    c.CreatedAt, c.FileUrl, c.FileName,
                    IsMine = c.UserId == userId
                })
        };

        return Ok(result);
    }

    /// <summary>
    /// Gruba yeni bir akademik görev ekler.
    /// Sadece Admin veya Moderatör rolündeki üyeler ekleyebilir (RBAC).
    /// POST /api/Groups/{groupId}/assignments
    /// </summary>
    [HttpPost("{groupId:guid}/assignments")]
    public async Task<IActionResult> CreateGroupAssignment(Guid groupId, [FromBody] CreateGroupAssignmentRequest request)
    {
        if (request == null || !ModelState.IsValid)
            return BadRequest("Görev bilgileri geçersiz.");

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        // RBAC: Sadece Admin veya Moderatör görev ekleyebilir
        var membership = await _db.GroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsApproved);

        if (membership == null) return Forbid();
        if (membership.Role == GroupRole.Member)
            return StatusCode(403, new { Message = "Görev ekleme yetkiniz yok. Sadece Admin veya Moderatör ekleyebilir." });

        try
        {
            var assignment = new GroupAssignment(
                groupId,
                userId,
                request.Title,
                request.CourseName,
                request.Type,
                request.DueDate.ToUniversalTime(),
                request.Description);

            _db.GroupAssignments.Add(assignment);
            await _db.SaveChangesAsync();

            return Ok(new { assignment.Id, assignment.Title, Message = "Görev başarıyla eklendi." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Görev eklenirken bir hata oluştu.", Details = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcının bir grup görevine kişisel notunu ve tahmini süresini ekler/günceller.
    /// Tüm onay lı üyeler kendi notunu güncelleyebilir.
    /// PATCH /api/Groups/{groupId}/assignments/{assignmentId}/notes
    /// </summary>
    [HttpPatch("{groupId:guid}/assignments/{assignmentId:guid}/notes")]
    public async Task<IActionResult> UpdateAssignmentNotes(
        Guid groupId, Guid assignmentId,
        [FromBody] UpdateAssignmentNotesRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var isMember = await _db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsApproved);
        if (!isMember) return Forbid();

        var assignment = await _db.GroupAssignments
            .FirstOrDefaultAsync(ga => ga.Id == assignmentId && ga.GroupId == groupId);
        if (assignment == null) return NotFound("Görev bulunamadı.");

        assignment.UpdateNotes(request.StudentNotes, request.EstimatedHours);
        await _db.SaveChangesAsync();

        return Ok(new { Message = "Notlar güncellendi." });
    }

    /// <summary>
    /// Kullanıcının bir grup görevini tamamlama durumunu toggle eder.
    /// Tamamlanmışsa iptal eder, tamamlanmamışsa tamamlandı olarak işaretler.
    /// POST /api/Groups/{groupId}/assignments/{assignmentId}/toggle-complete
    /// </summary>
    [HttpPost("{groupId:guid}/assignments/{assignmentId:guid}/toggle-complete")]
    public async Task<IActionResult> ToggleComplete(Guid groupId, Guid assignmentId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var isMember = await _db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsApproved);
        if (!isMember) return Forbid();

        var assignmentExists = await _db.GroupAssignments
            .AnyAsync(ga => ga.Id == assignmentId && ga.GroupId == groupId);
        if (!assignmentExists) return NotFound("Görev bulunamadı.");

        // Mevcut tamamlama kaydını ara
        var existing = await _db.AssignmentCompletions
            .FirstOrDefaultAsync(c => c.GroupAssignmentId == assignmentId && c.UserId == userId);

        bool isNowCompleted;

        if (existing != null)
        {
            // Tamamlandı olarak işaretliyse — iptal et
            _db.AssignmentCompletions.Remove(existing);
            isNowCompleted = false;
        }
        else
        {
            // Tamamlanmamış — tamamlandı olarak işaretle
            _db.AssignmentCompletions.Add(new AssignmentCompletion(assignmentId, userId));
            isNowCompleted = true;
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            IsCompleted = isNowCompleted,
            Message     = isNowCompleted ? "Görev tamamlandı olarak işaretlendi." : "Tamamlama işareti kaldırıldı."
        });
    }
    /// <summary>
    /// Grup görevine ait tartışma yorumlarını getirir.
    /// GET /api/Groups/{groupId}/assignments/{assignmentId}/comments
    /// </summary>
    [HttpGet("{groupId:guid}/assignments/{assignmentId:guid}/comments")]
    public async Task<IActionResult> GetAssignmentComments(Guid groupId, Guid assignmentId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var isMember = await _db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsApproved);
        if (!isMember) return Forbid();

        var comments = await _db.AssignmentComments
            .Where(c => c.GroupAssignmentId == assignmentId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new AssignmentCommentDto
            {
                Id        = c.Id,
                UserName  = c.UserName,
                Content   = c.Content,
                CreatedAt = c.CreatedAt,
                IsMine    = c.UserId == userId,
                FileUrl   = c.FileUrl,
                FileName  = c.FileName
            })
            .ToListAsync();

        return Ok(comments);
    }

    /// <summary>
    /// Grup görevine yeni bir tartışma yorumu / kaynak ekler.
    /// Dosya ekleme (PDF, Word, Görsel vb.) multipart/form-data olarak desteklenir.
    /// POST /api/Groups/{groupId}/assignments/{assignmentId}/comments
    /// </summary>
    [HttpPost("{groupId:guid}/assignments/{assignmentId:guid}/comments")]
    public async Task<IActionResult> CreateAssignmentComment(
        Guid groupId, Guid assignmentId,
        [FromForm] CreateAssignmentCommentDto request)
    {
        // İş Kuralı: Ya içerik ya da dosya zorunludur
        var hasContent = !string.IsNullOrWhiteSpace(request?.Content);
        var hasFile    = request?.File != null && request.File.Length > 0;

        if (!hasContent && !hasFile)
            return BadRequest("Yorum içeriği veya dosya eklenmesi zorunludur.");

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var isMember = await _db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsApproved);
        if (!isMember) return Forbid();

        // Görevin bu gruba ait olduğunu doğrula
        var assignmentExists = await _db.GroupAssignments
            .AnyAsync(ga => ga.Id == assignmentId && ga.GroupId == groupId);
        if (!assignmentExists) return NotFound("Grup görevi bulunamadı.");

        // Kullanıcı adını al
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return Unauthorized();

        // --- Dosya yükleme mantığı ---
        string? savedFileUrl  = null;
        string? savedFileName = null;

        if (hasFile)
        {
            // Dosya boyutu kontrolü: Max 10 MB
            const long maxSize = 10 * 1024 * 1024;
            if (request!.File!.Length > maxSize)
                return BadRequest("Dosya boyutu 10 MB'yi aşamaz.");

            // İzinli uzantılar
            var allowed  = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
                                   ".txt", ".jpg", ".jpeg", ".png", ".gif", ".webp", ".zip" };
            var ext      = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                return BadRequest($"'{ext}' uzantısı desteklenmiyor.");

            // Kaydedilecek klasor: wwwroot/uploads/{groupId}/
            var folderPath = Path.Combine(_env.WebRootPath, "uploads", groupId.ToString());
            Directory.CreateDirectory(folderPath);

            // Benzersiz dosya adı
            var uniqueName = $"{Guid.NewGuid()}{ext}";
            var fullPath   = Path.Combine(folderPath, uniqueName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            savedFileUrl  = $"/uploads/{groupId}/{uniqueName}";
            savedFileName = request.File.FileName; // Orijinal ad
        }

        // Yorum içeriği yoksa placeholder koy (dosya ekli)
        var content = hasContent ? request!.Content! : "📎 Dosya eklendi";

        var comment = new AssignmentComment(
            assignmentId,
            userId,
            user.Email ?? "Unknown User",
            content);

        if (savedFileUrl != null)
            comment.SetFile(savedFileUrl, savedFileName!);

        _db.AssignmentComments.Add(comment);
        await _db.SaveChangesAsync();

        var dto = new AssignmentCommentDto
        {
            Id        = comment.Id,
            UserName  = comment.UserName,
            Content   = comment.Content,
            CreatedAt = comment.CreatedAt,
            IsMine    = true,
            FileUrl   = comment.FileUrl,
            FileName  = comment.FileName
        };

        return Ok(dto);
    }

    /// <summary>
    /// Grubun vize/final haftası başlangıç tarihlerini günceller.
    /// Yalnızca grup Admin'i işlemi yapabilir.
    /// PATCH /api/Groups/{id}/exam-weeks
    /// </summary>
    [HttpPatch("{groupId:guid}/exam-weeks")]
    public async Task<IActionResult> UpdateExamWeeks(
        Guid groupId,
        [FromBody] UpdateExamWeeksRequest request)
    {
        var adminId = GetCurrentUserId();
        if (adminId == Guid.Empty) return Unauthorized();

        // Yetki kontrolü: Yalnızca Admin güncelleyebilir
        var isAdmin = await _db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == adminId
                         && gm.Role == GroupRole.Admin && gm.IsApproved);

        if (!isAdmin) return Forbid();

        var group = await _db.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null) return NotFound("Grup bulunamadı.");

        // UTC'ye çevirerek kaydet (nullable tarihler)
        var midtermStart = request.MidtermWeekStartDate.HasValue
            ? request.MidtermWeekStartDate.Value.ToUniversalTime() : (DateTime?)null;
        var midtermEnd   = request.MidtermWeekEndDate.HasValue
            ? request.MidtermWeekEndDate.Value.ToUniversalTime()   : (DateTime?)null;
        var finalStart   = request.FinalWeekStartDate.HasValue
            ? request.FinalWeekStartDate.Value.ToUniversalTime()   : (DateTime?)null;
        var finalEnd     = request.FinalWeekEndDate.HasValue
            ? request.FinalWeekEndDate.Value.ToUniversalTime()     : (DateTime?)null;
        var semesterEnd  = request.SemesterEndDate.HasValue
            ? request.SemesterEndDate.Value.ToUniversalTime()      : (DateTime?)null;
        var makeupStart  = request.MakeupExamsStartDate.HasValue
            ? request.MakeupExamsStartDate.Value.ToUniversalTime()   : (DateTime?)null;
        var makeupEnd    = request.MakeupExamsEndDate.HasValue
            ? request.MakeupExamsEndDate.Value.ToUniversalTime()     : (DateTime?)null;

        group.UpdateExamWeeks(midtermStart, midtermEnd, finalStart, finalEnd, semesterEnd, makeupStart, makeupEnd);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            Message = "Akademik takvim güncellendi.",
            group.MidtermWeekStartDate, group.MidtermWeekEndDate,
            group.FinalWeekStartDate,   group.FinalWeekEndDate,
            group.SemesterEndDate,      group.MakeupExamsStartDate, group.MakeupExamsEndDate
        });
    }

    /// <summary>
    /// Grup görevine yapılan tüm teslimleri getirir.
    /// GET /api/Groups/{groupId}/assignments/{assignmentId}/submissions
    /// </summary>
    [HttpGet("{groupId:guid}/assignments/{assignmentId:guid}/submissions")]
    public async Task<IActionResult> GetSubmissions(Guid groupId, Guid assignmentId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var isMember = await _db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsApproved);
        if (!isMember) return Forbid();

        var submissions = await _db.GroupAssignmentSubmissions
            .Where(s => s.GroupAssignmentId == assignmentId)
            .Include(s => s.Student)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new GroupAssignmentSubmissionResponseDto
            {
                Id = s.Id,
                StudentId = s.StudentId,
                StudentName = s.Student.Email ?? "Bilinmeyen Kullanıcı",
                MethodDescription = s.MethodDescription,
                ToolsUsed = s.ToolsUsed,
                ResultSummary = s.ResultSummary,
                FileUrl = s.FileUrl,
                CreatedAt = s.CreatedAt,
                IsMine = s.StudentId == userId
            })
            .ToListAsync();

        return Ok(submissions);
    }

    /// <summary>
    /// Grup görevine yeni bir çözüm teslimi ekler.
    /// POST /api/Groups/{groupId}/assignments/{assignmentId}/submissions
    /// </summary>
    [HttpPost("{groupId:guid}/assignments/{assignmentId:guid}/submissions")]
    public async Task<IActionResult> CreateSubmission(
        Guid groupId, Guid assignmentId,
        [FromBody] GroupAssignmentSubmissionCreateDto request)
    {
        if (request == null || !ModelState.IsValid)
            return BadRequest("Teslim bilgileri geçersiz.");

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var isMember = await _db.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsApproved);
        if (!isMember) return Forbid();

        var assignmentExists = await _db.GroupAssignments
            .AnyAsync(ga => ga.Id == assignmentId && ga.GroupId == groupId);
        if (!assignmentExists) return NotFound("Grup görevi bulunamadı.");

        var existingSubmission = await _db.GroupAssignmentSubmissions
            .FirstOrDefaultAsync(s => s.GroupAssignmentId == assignmentId && s.StudentId == userId);
        
        if (existingSubmission != null)
        {
            existingSubmission.UpdateSubmissionInfo(request.MethodDescription, request.ToolsUsed, request.ResultSummary, request.FileUrl);
        }
        else
        {
            var submission = new GroupAssignmentSubmission(
                assignmentId,
                userId,
                request.MethodDescription,
                request.ToolsUsed,
                request.ResultSummary,
                request.FileUrl);
            
            _db.GroupAssignmentSubmissions.Add(submission);
        }

        await _db.SaveChangesAsync();

        return Ok(new { Message = "Teslim başarıyla kaydedildi." });
    }

}

// DTO: Yeni grup oluştururken alınacak JSON gövdesi
public class CreateGroupRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Theme { get; set; }
    public GroupPrivacy Privacy { get; set; } = GroupPrivacy.Public;
}

// DTO: Yeni grup görevi eklerken alınacak JSON gövdesi
public class CreateGroupAssignmentRequest
{
    public string Title { get; set; } = default!;
    public string CourseName { get; set; } = default!;
    public GroupAssignmentType Type { get; set; } = GroupAssignmentType.Homework;
    public DateTime DueDate { get; set; }
    public string? Description { get; set; }
    public double? EstimatedHours { get; set; }
}

// DTO: Kişisel not ve tahmini süre güncellerken alınacak JSON gövdesi
public class UpdateAssignmentNotesRequest
{
    public string? StudentNotes   { get; set; }
    public double? EstimatedHours { get; set; }
}

public class AssignmentCommentDto
{
    public Guid     Id        { get; set; }
    public string   UserName  { get; set; } = default!;
    public string   Content   { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public bool     IsMine    { get; set; }
    public string?  FileUrl   { get; set; }
    public string?  FileName  { get; set; }
}

// Yorum ekleme DTO — multipart/form-data için IFormFile?
public class CreateAssignmentCommentDto
{
    public string?   Content { get; set; }
    public IFormFile? File   { get; set; }
}

public class JoinByCodeRequest
{
    public string Code { get; set; } = default!;
}

// DTO: Grup akademik takvimini güncellerken alınacak JSON gövdesi
public class UpdateExamWeeksRequest
{
    public DateTime? MidtermWeekStartDate { get; set; }
    public DateTime? MidtermWeekEndDate   { get; set; }
    public DateTime? FinalWeekStartDate   { get; set; }
    public DateTime? FinalWeekEndDate     { get; set; }
    public DateTime? SemesterEndDate      { get; set; }
    public DateTime? MakeupExamsStartDate { get; set; }
    public DateTime? MakeupExamsEndDate   { get; set; }
}

// DTO: Yeni değerlendirme/teslim kaydı eklerken alınacak JSON gövdesi
public class GroupAssignmentSubmissionCreateDto
{
    public string MethodDescription { get; set; } = default!;
    public string ToolsUsed { get; set; } = default!;
    public string ResultSummary { get; set; } = default!;
    public string? FileUrl { get; set; }
}

public class GroupAssignmentSubmissionResponseDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = default!;
    public string MethodDescription { get; set; } = default!;
    public string ToolsUsed { get; set; } = default!;
    public string ResultSummary { get; set; } = default!;
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsMine { get; set; }
}

