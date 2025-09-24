using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models.Entities
{
    [Table("project_status")]
    public class ProjectStatus
    {
        public ProjectStatus()
        {
            ProjectStatusName = string.Empty;
            ProjectStatusDescription = string.Empty;
        }

        [Key]
        [Column("project_status_id")]
        public int ProjectStatusId { get; set; }
        
        [Required]
        [MaxLength(255)]
        [Column("project_status_name")]
        public string ProjectStatusName { get; set; }
        
        [Column("project_status_description")]
        public string ProjectStatusDescription { get; set; }
        
        // Навигационные свойства
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}