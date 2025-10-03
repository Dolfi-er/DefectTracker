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
    public class DefectHistoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DefectHistoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DefectHistoryDTO>>> GetDefectHistories()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<DefectHistory> query = _context.DefectHistories
                .Include(dh => dh.User)
                .Include(dh => dh.Defect);

            // Наблюдатель видит только историю своих дефектов
            if (currentUserRole == "Observer")
            {
                query = query.Where(dh => dh.Defect.ResponsibleId == currentUserId);
            }

            return await query
                .Select(dh => new DefectHistoryDTO
                {
                    HistoryId = dh.HistoryId,
                    DefectId = dh.DefectId,
                    UserId = dh.UserId,
                    FieldName = dh.FieldName,
                    OldValue = dh.OldValue,
                    NewValue = dh.NewValue,
                    ChangeDate = dh.ChangeDate,
                    UserFio = dh.User!.Fio
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DefectHistoryDTO>> GetDefectHistory(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var defectHistory = await _context.DefectHistories
                .Include(dh => dh.User)
                .Include(dh => dh.Defect)
                .FirstOrDefaultAsync(dh => dh.HistoryId == id);

            if (defectHistory == null)
            {
                return NotFound();
            }

            // Наблюдатель может видеть только историю своих дефектов
            if (currentUserRole == "Observer" && defectHistory.Defect?.ResponsibleId != currentUserId)
            {
                return Forbid();
            }

            return new DefectHistoryDTO
            {
                HistoryId = defectHistory.HistoryId,
                DefectId = defectHistory.DefectId,
                UserId = defectHistory.UserId,
                FieldName = defectHistory.FieldName,
                OldValue = defectHistory.OldValue,
                NewValue = defectHistory.NewValue,
                ChangeDate = defectHistory.ChangeDate,
                UserFio = defectHistory.User!.Fio
            };
        }

        [HttpGet("defect/{defectId}")]
        public async Task<ActionResult<IEnumerable<DefectHistoryDTO>>> GetDefectHistoriesByDefect(int defectId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Проверяем доступ к дефекту
            var defect = await _context.Defects.FindAsync(defectId);
            if (defect == null)
            {
                return NotFound("Defect not found");
            }

            // Наблюдатель может видеть только историю своих дефектов
            if (currentUserRole == "Observer" && defect.ResponsibleId != currentUserId)
            {
                return Forbid();
            }

            return await _context.DefectHistories
                .Include(dh => dh.User)
                .Where(dh => dh.DefectId == defectId)
                .Select(dh => new DefectHistoryDTO
                {
                    HistoryId = dh.HistoryId,
                    DefectId = dh.DefectId,
                    UserId = dh.UserId,
                    FieldName = dh.FieldName,
                    OldValue = dh.OldValue,
                    NewValue = dh.NewValue,
                    ChangeDate = dh.ChangeDate,
                    UserFio = dh.User!.Fio
                })
                .ToListAsync();
        }
    }
}