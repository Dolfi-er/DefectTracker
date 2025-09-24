using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models.Entities
{
    [Table("info")]
    public class Info
    {
        public Info()
        {
            DefectName = string.Empty;
            DefectDescription = string.Empty;
        }

        [Key]
        [Column("info_id")]
        public int InfoId { get; set; }
        
        [Required]
        [MaxLength(255)]
        [Column("defect_name")]
        public string DefectName { get; set; }
        
        [Column("defect_description")]
        public string DefectDescription { get; set; }
        
        [Column("prioriyty")]
        public short Priority { get; set; }
        
        [Column("due_date")]
        public DateTime DueDate { get; set; }
        
        [Column("status_change_date")]
        public DateTime StatusChangeDate { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public Defect? Defect { get; set; }
    }
}