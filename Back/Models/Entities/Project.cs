using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models.Entities
{
    [Table("project")]
    public class Project
    {
        public Project()
        {
            Name = string.Empty;
            Description = string.Empty;
        }

        [Key]
        [Column("project_id")]
        public int ProjectId { get; set; }
        
        [Column("project_status_id")]
        public int ProjectStatusId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [Column("updated_date")]
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public ProjectStatus? ProjectStatus { get; set; }
        public ICollection<Defect> Defects { get; set; } = new List<Defect>();
    }
}