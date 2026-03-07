using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TBalans.Application.Services;
using TBalans.Domain.Entities;
using TBalans.Domain.Enums;

namespace TBalans.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;

    public AccountController(UserManager<User> userManager, ITokenService tokenService)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    /// <summary>
    /// Yeni kullanıcı kayıt uç noktası
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        try
        {
            if (request == null || !ModelState.IsValid)
                return BadRequest("Geçersiz istek.");

            // Clean Architecture kurgusu: Username olarak email'i kullan veya kullanıcıdan al.
            var user = new User(
                photoUrl: request.PhotoUrl ?? "",
                avatarType: (int)request.AvatarType,
                university: request.University,
                department: request.Department,
                academicYear: request.AcademicYear,
                semester: (int)request.Semester
            )
            {
                UserName = request.Email, // Identity standart
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Kullanıcı kaydı başarıyla oluşturuldu." });
            }

            return BadRequest(new { Errors = result.Errors });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Sunucu tarafında bir hata oluştu.", Error = ex.Message });
        }
    }

    /// <summary>
    /// Kullanıcı giriş ve Token oluşturma (Authentication)
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        try
        {
            if (request == null || !ModelState.IsValid)
                return BadRequest("Geçersiz istek.");

            var user = await _userManager.FindByEmailAsync(request.Email);
            
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return Unauthorized("Geçersiz e-posta veya şifre.");
            }

            // Başarılı giriş: Kullanıcıya JWT Token üretip döndürüyoruz
            var token = await _tokenService.GenerateJwtTokenAsync(user);

            return Ok(new 
            { 
                Token = token,
                Message = "Giriş başarılı.",
                // Login olduktan sonraki kritik bilgiler buraya dönülebilir (UX açısından)
                User = new { user.Id, user.Email, user.University, user.Department }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Sunucu tarafında bir hata oluştu.", Error = ex.Message });
        }
    }
}

// Data Transfer Objeleri (DTO)
public class RegisterDto
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    
    // TBalans projeye özel Business/Domain Alanları
    public string? PhotoUrl { get; set; }
    public AvatarType AvatarType { get; set; }
    public string University { get; set; } = default!;
    public string Department { get; set; } = default!;
    public string AcademicYear { get; set; } = default!;
    public Semester Semester { get; set; }
}

public class LoginDto
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}
