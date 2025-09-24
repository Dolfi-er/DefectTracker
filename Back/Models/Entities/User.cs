using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models.Entities
{
    [Table("user")]
    public class User
    {
        public User()
        {
            Login = string.Empty;
            Fio = string.Empty;
            Hash = string.Empty;
        }

        [Key]
        [Column("user_id")]
        public int UserId { get; set; }
        
        [Column("role_id")]
        public int RoleId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Login { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Fio { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Hash { get; set; }
        
        // Навигационные свойства
        public Role? Role { get; set; }
        public ICollection<Defect> ResponsibleDefects { get; set; } = new List<Defect>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<DefectHistory> DefectHistories { get; set; } = new List<DefectHistory>();
    }
}