using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back.Data;
using Back.Models.Entities;
using Back.DTOs;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetProjects()
        {
            return await _context.Projects
                .Include(p => p.ProjectStatus)
                .Select(p => new ProjectDTO
                {
                    ProjectId = p.ProjectId,
                    ProjectStatusId = p.ProjectStatusId,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate,
                    ProjectStatusName = p.ProjectStatus!.ProjectStatusName
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDTO>> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectStatus)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null)
            {
                return NotFound();
            }

            return new ProjectDTO
            {
                ProjectId = project.ProjectId,
                ProjectStatusId = project.ProjectStatusId,
                Name = project.Name,
                Description = project.Description,
                CreatedDate = project.CreatedDate,
                UpdatedDate = project.UpdatedDate,
                ProjectStatusName = project.ProjectStatus!.ProjectStatusName
            };
        }

        [HttpPost]
        public async Task<ActionResult<ProjectDTO>> CreateProject(ProjectCreateDTO projectCreateDTO)
        {
            var project = new Project
            {
                ProjectStatusId = projectCreateDTO.ProjectStatusId,
                Name = projectCreateDTO.Name,
                Description = projectCreateDTO.Description,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProject), new { id = project.ProjectId }, new ProjectDTO
            {
                ProjectId = project.ProjectId,
                ProjectStatusId = project.ProjectStatusId,
                Name = project.Name,
                Description = project.Description,
                CreatedDate = project.CreatedDate,
                UpdatedDate = project.UpdatedDate
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, ProjectUpdateDTO projectUpdateDTO)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            project.ProjectStatusId = projectUpdateDTO.ProjectStatusId;
            project.Name = projectUpdateDTO.Name;
            project.Description = projectUpdateDTO.Description;
            project.UpdatedDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.ProjectId == id);
        }
    }
}