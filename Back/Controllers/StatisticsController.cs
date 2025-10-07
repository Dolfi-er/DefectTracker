using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Back.Data;
using Back.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Back.DTOs;

namespace Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "CanViewAll")]
    public class StatisticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StatisticsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Statistics/overview
        [HttpGet("overview")]
        public async Task<ActionResult<OverviewStatsDTO>> GetOverviewStats()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<Defect> defectsQuery = _context.Defects;

            // Наблюдатель видит только свои дефекты
            if (currentUserRole == "Observer")
            {
                defectsQuery = defectsQuery.Where(d => d.ResponsibleId == currentUserId);
            }

            var totalDefects = await defectsQuery.CountAsync();
            
            var defectsByStatus = await defectsQuery
                .Include(d => d.Status)
                .GroupBy(d => d.StatusId)
                .Select(g => new { StatusId = g.Key, Count = g.Count() })
                .ToListAsync();

            var defectsByPriority = await defectsQuery
                .Include(d => d.Info)
                .GroupBy(d => d.Info != null ? d.Info.Priority : (short)0)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToListAsync();

            var recentDefects = await defectsQuery
                .OrderByDescending(d => d.CreatedDate)
                .Take(5)
                .Include(d => d.Project)
                .Include(d => d.Status)
                .Include(d => d.Responsible)
                .Include(d => d.Info)
                .Select(d => new DefectStatsDTO
                {
                    DefectId = d.DefectId,
                    DefectName = d.Info != null ? d.Info.DefectName : "Unknown",
                    ProjectName = d.Project != null ? d.Project.Name : "Unknown",
                    StatusName = d.Status != null ? d.Status.StatusName : "Unknown",
                    ResponsibleFio = d.Responsible != null ? d.Responsible.Fio : "Unassigned",
                    CreatedDate = d.CreatedDate,
                    Priority = d.Info != null ? d.Info.Priority : (short)0
                })
                .ToListAsync();

            return new OverviewStatsDTO
            {
                TotalDefects = totalDefects,
                DefectsByStatus = defectsByStatus.ToDictionary(x => x.StatusId, x => x.Count),
                DefectsByPriority = defectsByPriority.ToDictionary(x => x.Priority, x => x.Count),
                RecentDefects = recentDefects
            };
        }

        // GET: api/Statistics/defects-by-status
        [HttpGet("defects-by-status")]
        public async Task<ActionResult<IEnumerable<StatusStatsDTO>>> GetDefectsByStatus()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<Defect> defectsQuery = _context.Defects;

            // Наблюдатель видит только свои дефекты
            if (currentUserRole == "Observer")
            {
                defectsQuery = defectsQuery.Where(d => d.ResponsibleId == currentUserId);
            }

            var statusStats = await defectsQuery
                .Include(d => d.Status)
                .GroupBy(d => new { d.StatusId, StatusName = d.Status != null ? d.Status.StatusName : "Unknown" })
                .Select(g => new StatusStatsDTO
                {
                    StatusId = g.Key.StatusId,
                    StatusName = g.Key.StatusName,
                    Count = g.Count(),
                    Percentage = 0 // Рассчитаем позже
                })
                .ToListAsync();

            var total = statusStats.Sum(x => x.Count);
            foreach (var stat in statusStats)
            {
                stat.Percentage = total > 0 ? Math.Round((double)stat.Count / total * 100, 2) : 0;
            }

            return statusStats;
        }

        // GET: api/Statistics/defects-by-project
        [HttpGet("defects-by-project")]
        public async Task<ActionResult<IEnumerable<ProjectStatsDTO>>> GetDefectsByProject()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<Defect> defectsQuery = _context.Defects;

            // Наблюдатель видит только свои дефекты
            if (currentUserRole == "Observer")
            {
                defectsQuery = defectsQuery.Where(d => d.ResponsibleId == currentUserId);
            }

            var projectStats = await defectsQuery
                .Include(d => d.Project)
                .Include(d => d.Info)
                .GroupBy(d => new { d.ProjectId, ProjectName = d.Project != null ? d.Project.Name : "Unknown" })
                .Select(g => new ProjectStatsDTO
                {
                    ProjectId = g.Key.ProjectId,
                    ProjectName = g.Key.ProjectName,
                    TotalDefects = g.Count(),
                    OpenDefects = g.Count(d => d.StatusId != 4 && d.StatusId != 5), // Не закрытые и не отмененные
                    ClosedDefects = g.Count(d => d.StatusId == 4), // Закрытые
                    HighPriorityDefects = g.Count(d => d.Info != null && d.Info.Priority >= 3) // Приоритет 3 и выше считаем высоким
                })
                .OrderByDescending(p => p.TotalDefects)
                .ToListAsync();

            return projectStats;
        }

        // GET: api/Statistics/defects-by-user
        [HttpGet("defects-by-user")]
        [Authorize(Policy = "EngineerOrAbove")] // Только инженеры и менеджеры
        public async Task<ActionResult<IEnumerable<UserStatsDTO>>> GetDefectsByUser()
        {
            var userStats = await _context.Users
                .Where(u => u.RoleId == 2 || u.RoleId == 1) // Только инженеры и наблюдатели
                .Select(u => new UserStatsDTO
                {
                    UserId = u.UserId,
                    UserFio = u.Fio,
                    RoleName = u.Role != null ? u.Role.RoleName : "Unknown",
                    TotalAssignedDefects = u.ResponsibleDefects.Count,
                    OpenDefects = u.ResponsibleDefects.Count(d => d.StatusId != 4 && d.StatusId != 5),
                    ClosedDefects = u.ResponsibleDefects.Count(d => d.StatusId == 4),
                    OverdueDefects = u.ResponsibleDefects.Count(d => d.Info != null && 
                                                                   d.Info.DueDate < DateTime.UtcNow && 
                                                                   d.StatusId != 4 && d.StatusId != 5),
                    AverageCompletionDays = u.ResponsibleDefects
                        .Where(d => d.StatusId == 4)
                        .Average(d => d.UpdatedDate != default && d.CreatedDate != default ? 
                                    (d.UpdatedDate - d.CreatedDate).TotalDays : (double?)null)
                })
                .Where(u => u.TotalAssignedDefects > 0) // Только пользователи с назначенными дефектами
                .OrderByDescending(u => u.TotalAssignedDefects)
                .ToListAsync();

            return userStats;
        }

        // GET: api/Statistics/defects-timeline?days=30
        [HttpGet("defects-timeline")]
        public async Task<ActionResult<IEnumerable<TimelineStatsDTO>>> GetDefectsTimeline([FromQuery] int days = 30)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var startDate = DateTime.UtcNow.AddDays(-days);
            
            IQueryable<Defect> defectsQuery = _context.Defects.Where(d => d.CreatedDate >= startDate);

            // Наблюдатель видит только свои дефекты
            if (currentUserRole == "Observer")
            {
                defectsQuery = defectsQuery.Where(d => d.ResponsibleId == currentUserId);
            }

            var timelineStats = await defectsQuery
                .GroupBy(d => new { Date = d.CreatedDate.Date })
                .Select(g => new TimelineStatsDTO
                {
                    Date = g.Key.Date,
                    CreatedCount = g.Count(),
                    ClosedCount = g.Count(d => d.StatusId == 4)
                })
                .OrderBy(t => t.Date)
                .ToListAsync();

            // Заполняем пропущенные даты нулевыми значениями
            var completeTimeline = new List<TimelineStatsDTO>();
            for (var date = startDate.Date; date <= DateTime.UtcNow.Date; date = date.AddDays(1))
            {
                var existingStat = timelineStats.FirstOrDefault(t => t.Date == date);
                completeTimeline.Add(existingStat ?? new TimelineStatsDTO
                {
                    Date = date,
                    CreatedCount = 0,
                    ClosedCount = 0
                });
            }

            return completeTimeline;
        }

        // GET: api/Statistics/priority-metrics
        [HttpGet("priority-metrics")]
        public async Task<ActionResult<PriorityMetricsDTO>> GetPriorityMetrics()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<Defect> defectsQuery = _context.Defects;

            // Наблюдатель видит только свои дефекты
            if (currentUserRole == "Observer")
            {
                defectsQuery = defectsQuery.Where(d => d.ResponsibleId == currentUserId);
            }

            var priorityMetrics = await defectsQuery
                .Include(d => d.Info)
                .GroupBy(d => 1) // Группируем все записи
                .Select(g => new PriorityMetricsDTO
                {
                    TotalDefects = g.Count(),
                    AveragePriority = g.Average(d => d.Info != null ? (double)d.Info.Priority : 0),
                    HighPriorityCount = g.Count(d => d.Info != null && d.Info.Priority >= 4),
                    MediumPriorityCount = g.Count(d => d.Info != null && d.Info.Priority == 3),
                    LowPriorityCount = g.Count(d => d.Info != null && d.Info.Priority <= 2),
                    OverdueHighPriority = g.Count(d => d.Info != null && 
                                                     d.Info.Priority >= 4 && 
                                                     d.Info.DueDate < DateTime.UtcNow && 
                                                     d.StatusId != 4 && d.StatusId != 5)
                })
                .FirstOrDefaultAsync();

            return priorityMetrics ?? new PriorityMetricsDTO();
        }

        // GET: api/Statistics/project/{projectId}/details
        [HttpGet("project/{projectId}/details")]
        public async Task<ActionResult<ProjectDetailStatsDTO>> GetProjectDetailStats(int projectId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Проверяем существование проекта
            var project = await _context.Projects
                .Include(p => p.ProjectStatus)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
            {
                return NotFound("Project not found");
            }

            IQueryable<Defect> defectsQuery = _context.Defects.Where(d => d.ProjectId == projectId);

            // Наблюдатель видит только свои дефекты
            if (currentUserRole == "Observer")
            {
                defectsQuery = defectsQuery.Where(d => d.ResponsibleId == currentUserId);
            }

            var defects = await defectsQuery.ToListAsync();
            var defectsWithInfo = await defectsQuery.Include(d => d.Info).ToListAsync();

            var defectsByStatus = defects
                .GroupBy(d => d.StatusId)
                .ToDictionary(g => g.Key, g => g.Count());

            var oldestOpenDefect = defectsWithInfo
                .Where(d => d.StatusId != 4 && d.StatusId != 5 && d.Info != null)
                .OrderBy(d => d.CreatedDate)
                .FirstOrDefault();

            return new ProjectDetailStatsDTO
            {
                ProjectId = project.ProjectId,
                ProjectName = project.Name,
                ProjectStatus = project.ProjectStatus?.ProjectStatusName,
                TotalDefects = defects.Count,
                DefectsByStatus = defectsByStatus,
                AveragePriority = defectsWithInfo.Any(d => d.Info != null) ? 
                    defectsWithInfo.Where(d => d.Info != null).Average(d => d.Info.Priority) : 0,
                OldestOpenDefect = oldestOpenDefect?.Info?.DefectName,
                CompletionRate = defects.Any() ? 
                    (double)defects.Count(d => d.StatusId == 4) / defects.Count * 100 : 0
            };
        }
    }
}