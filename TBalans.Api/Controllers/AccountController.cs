using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;
using TBalans.Application.Services;
using TBalans.Domain.Entities;
using TBalans.Domain.Enums;

namespace TBalans.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService     _tokenService;

    public AccountController(UserManager<User> userManager, ITokenService tokenService)
    {
        _userManager  = userManager  ?? throw new ArgumentNullException(nameof(userManager));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    private Guid GetCurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }

    // ── POST /api/Account/register ───────────────────────────
    /// <summary>Yeni kullanıcı kayıt uç noktası</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        try
        {
            if (request == null || !ModelState.IsValid)
                return BadRequest("Geçersiz istek.");

            var user = new User(
                photoUrl:     request.PhotoUrl ?? "",
                avatarType:   (int)request.AvatarType,
                university:   request.University,
                department:   request.Department,
                academicYear: request.AcademicYear,
                semester:     (int)request.Semester)
            {
                UserName = request.Email,
                Email    = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
                return Ok(new { Message = "Kullanıcı kaydı başarıyla oluşturuldu." });

            return BadRequest(new { Errors = result.Errors });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Sunucu tarafında bir hata oluştu.", Error = ex.Message });
        }
    }

    // ── POST /api/Account/login ──────────────────────────────
    /// <summary>Kullanıcı girişi ve JWT Token oluşturma</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        try
        {
            if (request == null || !ModelState.IsValid)
                return BadRequest("Geçersiz istek.");

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return Unauthorized("Geçersiz e-posta veya şifre.");

            var token = await _tokenService.GenerateJwtTokenAsync(user);
            return Ok(new
            {
                Token   = token,
                Message = "Giriş başarılı.",
                User    = new
                {
                    user.Id, user.Email, user.University,
                    user.Department, user.Grade,
                    user.KarmaPoints, user.AvatarType, user.AcademicYear
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Sunucu tarafında bir hata oluştu.", Error = ex.Message });
        }
    }

    // ── POST /api/Account/google-login ───────────────────────
    /// <summary>Google OAuth2 ile giriş veya kayıt olma</summary>
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.Token))
                return BadRequest("Token eksik.");

            // 1. Google Token Doğrulaması (Google.Apis.Auth)
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token);
            
            // 2. Kullanıcı Veritabanı Kontrolü
            var user = await _userManager.FindByEmailAsync(payload.Email);

            // 3. Kullanıcı Yoksa Kayıt İşlemi (Register)
            if (user == null)
            {
                user = new User(
                    photoUrl: payload.Picture ?? "",
                    avatarType: 1, // Default olarak 1 atıyoruz
                    university: "Belirtilmedi",
                    department: "Belirtilmedi",
                    academicYear: "Belirtilmedi",
                    semester: 1)
                {
                    UserName = payload.Email,
                    Email = payload.Email
                    // İsim ve Soyisim özellikleri User domain sınıfına eklendiğinde
                    // payload.GivenName ve payload.FamilyName kullanılarak atanabilir.
                };

                // Geçerli bir şifre üretmek için (Güvenli ve Karmaşık Şifre Politikası gereği)
                var randomPassword = Guid.NewGuid().ToString("N") + "A1*";
                var result = await _userManager.CreateAsync(user, randomPassword);

                if (!result.Succeeded)
                    return BadRequest(new { Errors = result.Errors });
            }

            // 4. JWT Token Üretimi (Giriş veya yeni kayıt sonrasında ortak adım)
            var token = await _tokenService.GenerateJwtTokenAsync(user);

            return Ok(new
            {
                Token = token,
                Message = "Google ile başarılı giriş yapıldı.",
                User = new
                {
                    user.Id, user.Email, user.University,
                    user.Department, user.Grade,
                    user.KarmaPoints, user.AvatarType, user.AcademicYear
                }
            });
        }
        catch (InvalidJwtException ex)
        {
            return Unauthorized("Geçersiz Google Token: " + ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Sunucu tarafında bir hata oluştu.", Error = ex.Message });
        }
    }

    // ── GET /api/Account/me ──────────────────────────────────
    /// <summary>Giriş yapmış kullanıcının güncel bilgilerini döndürür</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var user = await _userManager.FindByIdAsync(GetCurrentUserId().ToString());
        if (user == null) return Unauthorized();

        return Ok(new
        {
            user.Id,
            user.Email,
            user.University,
            user.Department,
            user.Grade,
            user.AcademicYear,
            user.KarmaPoints,
            user.AvatarType
        });
    }

    // ── PUT /api/Account/profile ─────────────────────────────
    /// <summary>
    /// Giriş yapmış kullanıcının Üniversite, Bölüm, Sınıf ve Akademik Yıl bilgilerini günceller.
    /// SOLID — SRP: Güncelleme mantığı User.UpdateProfile() domain metoduna delege edilir.
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        if (dto == null) return BadRequest("Profil verisi eksik.");

        var user = await _userManager.FindByIdAsync(GetCurrentUserId().ToString());
        if (user == null) return Unauthorized();

        // Domain metoduna delege — enkapsülasyon korunuyor
        user.UpdateProfile(dto.University, dto.Department, dto.Grade, dto.AcademicYear);

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        return Ok(new
        {
            Message      = "Profil başarıyla güncellendi.",
            University   = user.University,
            Department   = user.Department,
            Grade        = user.Grade,
            AcademicYear = user.AcademicYear
        });
    }
}

// ── DTOs ────────────────────────────────────────────────────

public class RegisterDto
{
    public string     Email        { get; set; } = default!;
    public string     Password     { get; set; } = default!;
    public string?    PhotoUrl     { get; set; }
    public AvatarType AvatarType   { get; set; }
    public string     University   { get; set; } = default!;
    public string     Department   { get; set; } = default!;
    public string     AcademicYear { get; set; } = default!;
    public Semester   Semester     { get; set; }
}

public class LoginDto
{
    public string Email    { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class GoogleLoginDto
{
    public string Token { get; set; } = default!;
}

/// <summary>
/// Profil güncelleme isteği DTO'su. Tüm alanlar isteğe bağlıdır —
/// gönderilmeyenler UpdateProfile() domain metodunda görmezden gelinir.
/// </summary>
public class UpdateProfileDto
{
    public string? University   { get; set; }
    public string? Department   { get; set; }
    /// <summary>Sınıf bilgisi (Örn: "1", "2", "3", "4", "5+")</summary>
    public string? Grade        { get; set; }
    public string? AcademicYear { get; set; }
}
