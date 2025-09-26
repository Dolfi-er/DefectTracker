using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back.Data;
using Back.Models.Entities;
using Back.DTOs;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectStatusesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectStatusesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectStatusDTO>>> GetProjectStatuses()
        {
            return await _context.ProjectStatuses
                .Select(ps => new ProjectStatusDTO
                {
                    ProjectStatusId = ps.ProjectStatusId,
                    ProjectStatusName = ps.ProjectStatusName,
                    ProjectStatusDescription = ps.ProjectStatusDescription
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectStatusDTO>> GetProjectStatus(int id)
        {
            var projectStatus = await _context.ProjectStatuses.FindAsync(id);

            if (projectStatus == null)
            {
                return NotFound();
            }

            return new ProjectStatusDTO
            {
                ProjectStatusId = projectStatus.ProjectStatusId,
                ProjectStatusName = projectStatus.ProjectStatusName,
                ProjectStatusDescription = projectStatus.ProjectStatusDescription
            };
        }
    }
}