namespace Back.DTOs
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Fio { get; set; } = string.Empty;
        public string? RoleName { get; set; }
    }

    public class UserCreateDTO
    {
        public int RoleId { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Fio { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
    }

    public class UserUpdateDTO
    {
        public int RoleId { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Fio { get; set; } = string.Empty;
        public string? Hash { get; set; }
    }
}