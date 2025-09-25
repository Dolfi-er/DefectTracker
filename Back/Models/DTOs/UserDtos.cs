namespace Back.Models.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Fio { get; set; } = string.Empty;
        public RoleDto Role { get; set; } = new RoleDto();
        public DateTime CreatedDate { get; set; }
    }
    
    public class CreateUserDto
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Fio { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }
    
    public class UpdateUserDto
    {
        public string Fio { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }
    
    public class RoleDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}