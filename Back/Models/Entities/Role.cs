using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models.Entities
{
    [Table("role")]
    public class Role
    {
        public Role()
        {
            RoleName = string.Empty;
        }

        [Key]
        [Column("role_id")]
        public int RoleId { get; set; }
        
        [Required]
        [MaxLength(255)]
        [Column("role_name")]
        public string RoleName { get; set; }
        
        // Навигационные свойства
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}