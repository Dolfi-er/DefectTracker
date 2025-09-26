namespace Back.DTOs
{
    public class DefectStatusDTO
    {
        public int Id { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = string.Empty;
    }
}