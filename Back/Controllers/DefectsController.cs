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
    [Authorize(Policy = "CanViewAll")] // Все авторизованные могут просматривать
    public class DefectsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DefectsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DefectDTO>>> GetDefects()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<Defect> query = _context.Defects;

            // Наблюдатель видит только назначенные ему дефекты
            if (currentUserRole == "Observer")
            {
                query = query.Where(d => d.ResponsibleId == currentUserId);
            }

            return await query
                .Include(d => d.Project)
                .Include(d => d.Status)
                .Include(d => d.Info)
                .Include(d => d.Responsible)
                .Include(d => d.CreatedBy)
                .Select(d => new DefectDTO
                {
                    DefectId = d.DefectId,
                    ProjectId = d.ProjectId,
                    StatusId = d.StatusId,
                    InfoId = d.InfoId,
                    ResponsibleId = d.ResponsibleId,
                    CreatedById = d.CreatedById,
                    CreatedDate = d.CreatedDate,
                    UpdatedDate = d.UpdatedDate,
                    ProjectName = d.Project!.Name,
                    StatusName = d.Status!.StatusName,
                    ResponsibleFio = d.Responsible!.Fio,
                    CreatedByFio = d.CreatedBy!.Fio,
                    Info = d.Info != null ? new InfoDTO
                    {
                        InfoId = d.Info.InfoId,
                        DefectName = d.Info.DefectName,
                        DefectDescription = d.Info.DefectDescription,
                        Priority = d.Info.Priority,
                        DueDate = d.Info.DueDate,
                        StatusChangeDate = d.Info.StatusChangeDate
                    } : null
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DefectDTO>> GetDefect(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var defect = await _context.Defects
                .Include(d => d.Project)
                .Include(d => d.Status)
                .Include(d => d.Info)
                .Include(d => d.Responsible)
                .Include(d => d.CreatedBy)
                .FirstOrDefaultAsync(d => d.DefectId == id);

            if (defect == null)
            {
                return NotFound();
            }

            // Наблюдатель может видеть только назначенные ему дефекты
            if (currentUserRole == "Observer" && defect.ResponsibleId != currentUserId)
            {
                return Forbid();
            }

            return new DefectDTO
            {
                DefectId = defect.DefectId,
                ProjectId = defect.ProjectId,
                StatusId = defect.StatusId,
                InfoId = defect.InfoId,
                ResponsibleId = defect.ResponsibleId,
                CreatedById = defect.CreatedById,
                CreatedDate = defect.CreatedDate,
                UpdatedDate = defect.UpdatedDate,
                ProjectName = defect.Project!.Name,
                StatusName = defect.Status!.StatusName,
                ResponsibleFio = defect.Responsible!.Fio,
                CreatedByFio = defect.CreatedBy!.Fio,
                Info = defect.Info != null ? new InfoDTO
                {
                    InfoId = defect.Info.InfoId,
                    DefectName = defect.Info.DefectName,
                    DefectDescription = defect.Info.DefectDescription,
                    Priority = defect.Info.Priority,
                    DueDate = defect.Info.DueDate,
                    StatusChangeDate = defect.Info.StatusChangeDate
                } : null
            };
        }

        [HttpPost]
        [Authorize(Policy = "CanManageDefects")]
        public async Task<ActionResult<DefectDTO>> CreateDefect(DefectCreateDTO defectCreateDTO)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Сначала создаем Info
            var info = new Info
            {
                DefectName = defectCreateDTO.Info.DefectName,
                DefectDescription = defectCreateDTO.Info.DefectDescription,
                Priority = defectCreateDTO.Info.Priority,
                DueDate = defectCreateDTO.Info.DueDate.ToUniversalTime(), // Преобразуем в UTC
                StatusChangeDate = DateTime.UtcNow
            };

            _context.Infos.Add(info);
            await _context.SaveChangesAsync();

            var defect = new Defect
            {
                ProjectId = defectCreateDTO.ProjectId,
                StatusId = defectCreateDTO.StatusId,
                InfoId = info.InfoId,
                ResponsibleId = defectCreateDTO.ResponsibleId,
                CreatedById = currentUserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Defects.Add(defect);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDefect), new { id = defect.DefectId }, new DefectDTO
            {
                DefectId = defect.DefectId,
                ProjectId = defect.ProjectId,
                StatusId = defect.StatusId,
                InfoId = defect.InfoId,
                ResponsibleId = defect.ResponsibleId,
                CreatedById = defect.CreatedById,
                CreatedDate = defect.CreatedDate,
                UpdatedDate = defect.UpdatedDate
            });
        }


        [HttpPut("{id}")]
        [Authorize(Policy = "CanManageDefects")]
        public async Task<IActionResult> UpdateDefect(int id, DefectUpdateDTO defectUpdateDTO)
        {
            var defect = await _context.Defects
                .Include(d => d.Info)
                .FirstOrDefaultAsync(d => d.DefectId == id);

            if (defect == null)
            {
                return NotFound();
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Инженер может редактировать только назначенные ему дефекты
            if (currentUserRole == "Engineer" && defect.ResponsibleId != currentUserId)
            {
                return Forbid();
            }

            defect.ProjectId = defectUpdateDTO.ProjectId;
            defect.StatusId = defectUpdateDTO.StatusId;
            defect.ResponsibleId = defectUpdateDTO.ResponsibleId;
            defect.UpdatedDate = DateTime.UtcNow;

            // Обновляем связанную информацию
            if (defect.Info != null)
            {
                defect.Info.DefectName = defectUpdateDTO.Info.DefectName;
                defect.Info.DefectDescription = defectUpdateDTO.Info.DefectDescription;
                defect.Info.Priority = defectUpdateDTO.Info.Priority;
                defect.Info.DueDate = defectUpdateDTO.Info.DueDate.ToUniversalTime(); // Преобразуем в UTC
                defect.Info.StatusChangeDate = DateTime.UtcNow;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DefectExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }
        
        [HttpDelete("{id}")]
        [Authorize(Policy = "CanManageDefects")] // Инженер и менеджер могут удалять дефекты
        public async Task<IActionResult> DeleteDefect(int id)
        {
            var defect = await _context.Defects.FindAsync(id);
            if (defect == null)
            {
                return NotFound();
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value); // Fixed: value -> Value
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Инженер может удалять только назначенные ему дефекты
            if (currentUserRole == "Engineer" && defect.ResponsibleId != currentUserId)
            {
                return Forbid();
            }

            _context.Defects.Remove(defect);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DefectExists(int id)
        {
            return _context.Defects.Any(e => e.DefectId == id);
        }
    }
}