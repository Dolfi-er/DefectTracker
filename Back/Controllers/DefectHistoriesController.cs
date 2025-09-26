using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back.Data;
using Back.Models.Entities;
using Back.DTOs;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            return await _context.DefectHistories
                .Include(dh => dh.User)
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
            var defectHistory = await _context.DefectHistories
                .Include(dh => dh.User)
                .FirstOrDefaultAsync(dh => dh.HistoryId == id);

            if (defectHistory == null)
            {
                return NotFound();
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