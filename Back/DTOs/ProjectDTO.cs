namespace Back.DTOs
{
    public class ProjectDTO
    {
        public int ProjectId { get; set; }
        public int ProjectStatusId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string? ProjectStatusName { get; set; }
    }

    public class ProjectCreateDTO
    {
        public int ProjectStatusId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class ProjectUpdateDTO
    {
        public int ProjectStatusId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}