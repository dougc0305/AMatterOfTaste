using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AMatterOfTaste.Api.Data;
using AMatterOfTaste.Api.Models.DTOs;
using AMatterOfTaste.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AMatterOfTaste.Api.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _db.AppUsers
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        return new AuthResponseDto
        {
            Token = GenerateToken(user),
            User = MapToDto(user)
        };
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        if (await _db.AppUsers.AnyAsync(u => u.Email == dto.Email))
            return null;

        var user = new AppUser
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsAdmin = false,
            IsActive = true,
            CreatedById = 1,
            ModifiedById = 1,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = GenerateToken(user),
            User = MapToDto(user)
        };
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _db.AppUsers
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.ModifiedById = userId;
        user.ModifiedDate = DateTime.Now;

        await _db.SaveChangesAsync();
        return true;
    }

    private string GenerateToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name)
        };

        if (user.IsAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static UserDto MapToDto(AppUser user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        CookbookSlug = user.CookbookSlug,
        IsAdmin = user.IsAdmin
    };
}
