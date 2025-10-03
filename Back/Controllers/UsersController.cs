using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back.Data;
using Back.Models.Entities;
using Back.DTOs;
using Back.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "CanManageUsers")] // Только менеджер может управлять пользователями
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService; // Авторизация сервис

        public UsersController(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Select(u => new UserDTO
                {
                    UserId = u.UserId,
                    RoleId = u.RoleId,
                    Login = u.Login,
                    Fio = u.Fio,
                    RoleName = u.Role!.RoleName
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return new UserDTO
            {
                UserId = user.UserId,
                RoleId = user.RoleId,
                Login = user.Login,
                Fio = user.Fio,
                RoleName = user.Role!.RoleName
            };
        }

        [HttpPost]
        public async Task<ActionResult<UserDTO>> CreateUser(UserCreateDTO userCreateDTO)
        {
            // Проверяем, что создающий пользователь - менеджер
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUserRole != "Manager")
            {
                return Forbid();
            }

            // Не позволяем создавать пользователей с ролью выше своей
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUser = await _context.Users.FindAsync(currentUserId);
            
            if (userCreateDTO.RoleId > currentUser!.RoleId)
            {
                return BadRequest("Cannot create user with higher role");
            }

            var user = new User
            {
                RoleId = userCreateDTO.RoleId,
                Login = userCreateDTO.Login,
                Fio = userCreateDTO.Fio,
                Hash = _authService.HashPassword(userCreateDTO.Hash)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, new UserDTO
            {
                UserId = user.UserId,
                RoleId = user.RoleId,
                Login = user.Login,
                Fio = user.Fio
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserUpdateDTO userUpdateDTO)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Проверяем права доступа
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Пользователь может редактировать только себя, если он не менеджер
            if (currentUserRole != "Manager" && currentUserId != id)
            {
                return Forbid();
            }

            // Менеджер не может изменить роль на более высокую
            if (currentUserRole == "Manager" && userUpdateDTO.RoleId > user.RoleId)
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (userUpdateDTO.RoleId > currentUser!.RoleId)
                {
                    return BadRequest("Cannot assign higher role than your own");
                }
            }

            user.RoleId = userUpdateDTO.RoleId;
            user.Login = userUpdateDTO.Login;
            user.Fio = userUpdateDTO.Fio;
            
            if (!string.IsNullOrEmpty(userUpdateDTO.Hash))
            {
                user.Hash = _authService.HashPassword(userUpdateDTO.Hash);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Нельзя удалить самого себя
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (currentUserId == id)
            {
                return BadRequest("Cannot delete your own account");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}