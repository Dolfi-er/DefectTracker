namespace Back.DTOs
{
    public class OverviewStatsDTO
    {
        public int TotalDefects { get; set; }
        public Dictionary<int, int> DefectsByStatus { get; set; } = new Dictionary<int, int>();
        public Dictionary<short, int> DefectsByPriority { get; set; } = new Dictionary<short, int>();
        public List<DefectStatsDTO> RecentDefects { get; set; } = new List<DefectStatsDTO>();
    }

    public class DefectStatsDTO
    {
        public int DefectId { get; set; }
        public string DefectName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string ResponsibleFio { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public short Priority { get; set; }
    }

    public class StatusStatsDTO
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class ProjectStatsDTO
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int TotalDefects { get; set; }
        public int OpenDefects { get; set; }
        public int ClosedDefects { get; set; }
        public int HighPriorityDefects { get; set; }
    }

    public class UserStatsDTO
    {
        public int UserId { get; set; }
        public string UserFio { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int TotalAssignedDefects { get; set; }
        public int OpenDefects { get; set; }
        public int ClosedDefects { get; set; }
        public int OverdueDefects { get; set; }
        public double? AverageCompletionDays { get; set; }
    }

    public class TimelineStatsDTO
    {
        public DateTime Date { get; set; }
        public int CreatedCount { get; set; }
        public int ClosedCount { get; set; }
    }

    public class PriorityMetricsDTO
    {
        public int TotalDefects { get; set; }
        public double AveragePriority { get; set; }
        public int HighPriorityCount { get; set; }
        public int MediumPriorityCount { get; set; }
        public int LowPriorityCount { get; set; }
        public int OverdueHighPriority { get; set; }
    }

    public class ProjectDetailStatsDTO
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? ProjectStatus { get; set; }
        public int TotalDefects { get; set; }
        public Dictionary<int, int> DefectsByStatus { get; set; } = new Dictionary<int, int>();
        public double AveragePriority { get; set; }
        public string? OldestOpenDefect { get; set; }
        public double CompletionRate { get; set; }
    }
}