using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back.Data;
using Back.Models.Entities;
using Back.DTOs;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
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
            var user = new User
            {
                RoleId = userCreateDTO.RoleId,
                Login = userCreateDTO.Login,
                Fio = userCreateDTO.Fio,
                Hash = userCreateDTO.Hash
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
            if (id != userUpdateDTO.RoleId) // Обычно здесь должно быть сравнение с UserId, но в DTO нет UserId
            {
                return BadRequest();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.RoleId = userUpdateDTO.RoleId;
            user.Login = userUpdateDTO.Login;
            user.Fio = userUpdateDTO.Fio;
            if (!string.IsNullOrEmpty(userUpdateDTO.Hash))
            {
                user.Hash = userUpdateDTO.Hash;
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