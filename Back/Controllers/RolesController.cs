using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back.Data;
using Back.Models.Entities;
using Back.DTOs;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDTO>>> GetRoles()
        {
            return await _context.Roles
                .Select(r => new RoleDTO
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDTO>> GetRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            return new RoleDTO
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName
            };
        }
    }
}