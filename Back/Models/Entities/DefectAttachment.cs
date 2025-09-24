using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models.Entities
{
    [Table("defect_attachment")]
    public class DefectAttachment
    {
        public DefectAttachment()
        {
            FileName = string.Empty;
            FilePath = string.Empty;
        }

        [Key]
        [Column("attachment_id")]
        public int AttachmentId { get; set; }
        
        [Column("defect_id")]
        public int DefectId { get; set; }
        
        [Required]
        [MaxLength(255)]
        [Column("file_name")]
        public string FileName { get; set; }
        
        [Required]
        [MaxLength(500)]
        [Column("file_path")]
        public string FilePath { get; set; }
        
        [Column("file_size")]
        public long FileSize { get; set; }
        
        [Column("upload_date")]
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        
        [Column("uploaded_by_id")]
        public int UploadedById { get; set; }
        
        // Навигационные свойства
        public Defect? Defect { get; set; }
        public User? UploadedBy { get; set; }
    }
}