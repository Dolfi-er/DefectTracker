using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models.Entities
{
    [Table("defect_history")]
    public class DefectHistory
    {
        public DefectHistory()
        {
            FieldName = string.Empty;
            OldValue = string.Empty;
            NewValue = string.Empty;
        }

        [Key]
        [Column("history_id")]
        public int HistoryId { get; set; }
        
        [Column("defect_id")]
        public int DefectId { get; set; }
        
        [Column("user_id")]
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(100)]
        [Column("field_name")]
        public string FieldName { get; set; }
        
        [Column("old_value")]
        public string OldValue { get; set; }
        
        [Column("new_value")]
        public string NewValue { get; set; }
        
        [Column("change_date")]
        public DateTime ChangeDate { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public Defect? Defect { get; set; }
        public User? User { get; set; }
    }
}