namespace Back.DTOs
{
    public class ProjectStatusDTO
    {
        public int ProjectStatusId { get; set; }
        public string ProjectStatusName { get; set; } = string.Empty;
        public string ProjectStatusDescription { get; set; } = string.Empty;
    }
}