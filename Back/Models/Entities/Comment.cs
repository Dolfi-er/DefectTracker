using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models.Entities
{
    [Table("comment")]
    public class Comment
    {
        public Comment()
        {
            CommentText = string.Empty;
        }

        [Key]
        [Column("comment_id")]
        public int CommentId { get; set; }
        
        [Column("defect_id")]
        public int DefectId { get; set; }
        
        [Column("user_id")]
        public int UserId { get; set; }
        
        [Required]
        [Column("comment_text")]
        public string CommentText { get; set; }
        
        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;
        
        // Навигационные свойства
        public Defect? Defect { get; set; }
        public User? User { get; set; }
    }
}