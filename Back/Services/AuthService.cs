using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Back.Data;
using Back.DTOs;
using Back.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Back.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthService(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Login == loginDTO.Login);

            if (user == null || !VerifyPassword(loginDTO.Password, user.Hash))
            {
                throw new UnauthorizedAccessException("Invalid login or password");
            }

            var token = GenerateJwtToken(user);
            var expires = DateTime.UtcNow.AddHours(24); // Токен на 24 часа

            return new AuthResponseDTO
            {
                Token = token,
                User = new UserDTO
                {
                    UserId = user.UserId,
                    Login = user.Login,
                    Fio = user.Fio,
                    RoleId = user.RoleId,
                    RoleName = user.Role?.RoleName
                },
                Expires = expires
            };
        }

        public async Task<UserDTO> RegisterAsync(RegisterDTO registerDTO, int createdById)
        {
            // Проверяем, существует ли пользователь с таким логином
            if (await _context.Users.AnyAsync(u => u.Login == registerDTO.Login))
            {
                throw new ArgumentException("User with this login already exists");
            }

            var user = new User
            {
                Login = registerDTO.Login,
                Fio = registerDTO.Fio,
                Hash = HashPassword(registerDTO.Password),
                RoleId = registerDTO.RoleId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var createdUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == user.UserId);

            return new UserDTO
            {
                UserId = createdUser!.UserId,
                Login = createdUser.Login,
                Fio = createdUser.Fio,
                RoleId = createdUser.RoleId,
                RoleName = createdUser.Role?.RoleName
            };
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.GivenName, user.Fio),
                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Observer")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}