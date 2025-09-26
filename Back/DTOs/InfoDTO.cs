namespace Back.DTOs
{
    public class InfoDTO
    {
        public int InfoId { get; set; }
        public string DefectName { get; set; } = string.Empty;
        public string DefectDescription { get; set; } = string.Empty;
        public short Priority { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime StatusChangeDate { get; set; }
    }

    public class InfoCreateDTO
    {
        public string DefectName { get; set; } = string.Empty;
        public string DefectDescription { get; set; } = string.Empty;
        public short Priority { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class InfoUpdateDTO
    {
        public string DefectName { get; set; } = string.Empty;
        public string DefectDescription { get; set; } = string.Empty;
        public short Priority { get; set; }
        public DateTime DueDate { get; set; }
    }
}