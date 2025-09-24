using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models.Entities
{
    [Table("defect_status")]
    public class DefectStatus
    {
        public DefectStatus()
        {
            StatusName = string.Empty;
            StatusDescription = string.Empty;
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        [Column("status_name")]
        public string StatusName { get; set; }
        
        [Column("status_desc")]
        public string StatusDescription { get; set; }
        
        // Навигационные свойства
        public ICollection<Defect> Defects { get; set; } = new List<Defect>();
    }
}