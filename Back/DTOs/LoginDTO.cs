namespace Back.DTOs
{
    public class LoginDTO
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public UserDTO User { get; set; } = new UserDTO();
        public DateTime Expires { get; set; }
    }

    public class RegisterDTO
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Fio { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }
}