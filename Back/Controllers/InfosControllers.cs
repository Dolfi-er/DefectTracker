using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back.Data;
using Back.Models.Entities;
using Back.DTOs;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            return await _context.Infos
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
            var info = await _context.Infos.FindAsync(id);

            if (info == null)
            {
                return NotFound();
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
        public async Task<IActionResult> UpdateInfo(int id, InfoUpdateDTO infoUpdateDTO)
        {
            var info = await _context.Infos.FindAsync(id);
            if (info == null)
            {
                return NotFound();
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
        public async Task<IActionResult> DeleteInfo(int id)
        {
            var info = await _context.Infos.FindAsync(id);
            if (info == null)
            {
                return NotFound();
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