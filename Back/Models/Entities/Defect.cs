using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models.Entities
{
    [Table("defect")]
    public class Defect
    {
        [Key]
        [Column("defect_id")]
        public int DefectId { get; set; }
        
        [Column("project_id")]
        public int ProjectId { get; set; }
        
        [Column("status_id")]
        public int StatusId { get; set; }
        
        [Column("info_id")]
        public int InfoId { get; set; }
        
        [Column("responsible_id")]
        public int? ResponsibleId { get; set; }
        
        [Column("created_by_id")]
        public int CreatedById { get; set; }
        
        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [Column("updated_date")]
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public Project? Project { get; set; }
        public DefectStatus? Status { get; set; }
        public Info? Info { get; set; }
        public User? Responsible { get; set; }
        public User? CreatedBy { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<DefectHistory> History { get; set; } = new List<DefectHistory>();
        public ICollection<DefectAttachment> Attachments { get; set; } = new List<DefectAttachment>();
    }
}