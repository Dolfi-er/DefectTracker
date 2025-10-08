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
        public string DefectName { get; set; } = null!;
        public string DefectDescription { get; set; } = null!;
        public short Priority { get; set; }
        public DateTime DueDate { get; set; } // Измените на DateTime
    }

    public class InfoUpdateDTO
    {
        public string DefectName { get; set; } = null!;
        public string DefectDescription { get; set; } = null!;
        public short Priority { get; set; }
        public DateTime DueDate { get; set; } // Измените на DateTime
    }
}