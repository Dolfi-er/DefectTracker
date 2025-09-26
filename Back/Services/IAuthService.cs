using Back.DTOs;
using Back.Models.Entities;

namespace Back.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> LoginAsync(LoginDTO loginDTO);
        Task<UserDTO> RegisterAsync(RegisterDTO registerDTO, int createdById);
        string GenerateJwtToken(User user);
        bool VerifyPassword(string password, string hash);
        string HashPassword(string password);
    }
}