using Microsoft.AspNetCore.Mvc;
using Back.Services;
using Back.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginDTO loginDTO)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDTO);

                // Устанавливаем куку с токеном
                Response.Cookies.Append("access_token", result.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !_configuration.GetValue<bool>("IsDevelopment"), // В разработке false для HTTP
                    SameSite = SameSiteMode.Strict,
                    Expires = result.Expires,
                    Path = "/"
                });

                return Ok(new 
                { 
                    user = result.User,
                    expires = result.Expires
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid login or password");
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // Удаляем куку
            Response.Cookies.Delete("access_token", new CookieOptions
            {
                Path = "/"
            });

            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("register")]
        [Authorize(Policy = "CanManageUsers")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            try
            {
                var createdById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var result = await _authService.RegisterAsync(registerDTO, createdById);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("me")]
        [Authorize]
        public ActionResult<UserDTO> GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var login = User.FindFirst(ClaimTypes.Name)!.Value;
            var fio = User.FindFirst(ClaimTypes.GivenName)!.Value;
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            // Получаем roleId из базы данных (упрощенно)
            var roleId = role switch
            {
                "Manager" => 3,
                "Engineer" => 2,
                "Observer" => 1,
                _ => 1
            };

            return Ok(new UserDTO
            {
                UserId = userId,
                Login = login,
                Fio = fio,
                RoleId = roleId,
                RoleName = role
            });
        }
    }
}