namespace Back.Models.DTOs
{
    public class ProjectDto
    {
        public int ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ProjectStatusDto Status { get; set; } = new ProjectStatusDto();
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int DefectCount { get; set; }
    }
    
    public class CreateProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ProjectStatusId { get; set; } = 1; // По умолчанию Active
    }
    
    public class UpdateProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ProjectStatusId { get; set; }
    }
    
    public class ProjectStatusDto
    {
        public int ProjectStatusId { get; set; }
        public string ProjectStatusName { get; set; } = string.Empty;
        public string ProjectStatusDescription { get; set; } = string.Empty;
    }
}