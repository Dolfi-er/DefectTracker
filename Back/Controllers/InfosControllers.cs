using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back.Data;
using Back.Models.Entities;
using Back.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "CanViewAll")]
    public class InfosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InfosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InfoDTO>>> GetInfos()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<Info> query = _context.Infos
                .Include(i => i.Defect);

            // Наблюдатель видит только информацию о своих дефектах
            if (currentUserRole == "Observer")
            {
                query = query.Where(i => i.Defect!.ResponsibleId == currentUserId);
            }

            return await query
                .Select(i => new InfoDTO
                {
                    InfoId = i.InfoId,
                    DefectName = i.DefectName,
                    DefectDescription = i.DefectDescription,
                    Priority = i.Priority,
                    DueDate = i.DueDate,
                    StatusChangeDate = i.StatusChangeDate
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InfoDTO>> GetInfo(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var info = await _context.Infos
                .Include(i => i.Defect)
                .FirstOrDefaultAsync(i => i.InfoId == id);

            if (info == null)
            {
                return NotFound();
            }

            // Наблюдатель может видеть только информацию о своих дефектах
            if (currentUserRole == "Observer" && info.Defect?.ResponsibleId != currentUserId)
            {
                return Forbid();
            }

            return new InfoDTO
            {
                InfoId = info.InfoId,
                DefectName = info.DefectName,
                DefectDescription = info.DefectDescription,
                Priority = info.Priority,
                DueDate = info.DueDate,
                StatusChangeDate = info.StatusChangeDate
            };
        }

        [HttpPost]
        [Authorize(Policy = "CanManageDefects")]
        public async Task<ActionResult<InfoDTO>> CreateInfo(InfoCreateDTO infoCreateDTO)
        {
            var info = new Info
            {
                DefectName = infoCreateDTO.DefectName,
                DefectDescription = infoCreateDTO.DefectDescription,
                Priority = infoCreateDTO.Priority,
                DueDate = infoCreateDTO.DueDate,
                StatusChangeDate = DateTime.UtcNow
            };

            _context.Infos.Add(info);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInfo), new { id = info.InfoId }, new InfoDTO
            {
                InfoId = info.InfoId,
                DefectName = info.DefectName,
                DefectDescription = info.DefectDescription,
                Priority = info.Priority,
                DueDate = info.DueDate,
                StatusChangeDate = info.StatusChangeDate
            });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "CanManageDefects")]
        public async Task<IActionResult> UpdateInfo(int id, InfoUpdateDTO infoUpdateDTO)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var info = await _context.Infos
                .Include(i => i.Defect)
                .FirstOrDefaultAsync(i => i.InfoId == id);

            if (info == null)
            {
                return NotFound();
            }

            // Наблюдатель может редактировать только информацию о своих дефектах
            if (currentUserRole == "Observer" && info.Defect?.ResponsibleId != currentUserId)
            {
                return Forbid();
            }

            info.DefectName = infoUpdateDTO.DefectName;
            info.DefectDescription = infoUpdateDTO.DefectDescription;
            info.Priority = infoUpdateDTO.Priority;
            info.DueDate = infoUpdateDTO.DueDate;
            info.StatusChangeDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InfoExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "CanManageDefects")]
        public async Task<IActionResult> DeleteInfo(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var info = await _context.Infos
                .Include(i => i.Defect)
                .FirstOrDefaultAsync(i => i.InfoId == id);

            if (info == null)
            {
                return NotFound();
            }

            // Наблюдатель может удалять только информацию о своих дефектах
            if (currentUserRole == "Observer" && info.Defect?.ResponsibleId != currentUserId)
            {
                return Forbid();
            }

            _context.Infos.Remove(info);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InfoExists(int id)
        {
            return _context.Infos.Any(e => e.InfoId == id);
        }
    }
}