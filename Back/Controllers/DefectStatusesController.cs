using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back.Data;
using Back.Models.Entities;
using Back.DTOs;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DefectStatusesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DefectStatusesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DefectStatusDTO>>> GetDefectStatuses()
        {
            return await _context.DefectStatuses
                .Select(ds => new DefectStatusDTO
                {
                    Id = ds.Id,
                    StatusName = ds.StatusName,
                    StatusDescription = ds.StatusDescription
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DefectStatusDTO>> GetDefectStatus(int id)
        {
            var defectStatus = await _context.DefectStatuses.FindAsync(id);

            if (defectStatus == null)
            {
                return NotFound();
            }

            return new DefectStatusDTO
            {
                Id = defectStatus.Id,
                StatusName = defectStatus.StatusName,
                StatusDescription = defectStatus.StatusDescription
            };
        }
    }
}