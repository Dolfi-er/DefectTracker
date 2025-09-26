namespace Back.DTOs
{
    public class DefectDTO
    {
        public int DefectId { get; set; }
        public int ProjectId { get; set; }
        public int StatusId { get; set; }
        public int InfoId { get; set; }
        public int? ResponsibleId { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        
        // Навигационные свойства
        public string? ProjectName { get; set; }
        public string? StatusName { get; set; }
        public string? ResponsibleFio { get; set; }
        public string? CreatedByFio { get; set; }
        public InfoDTO? Info { get; set; }
    }

    public class DefectCreateDTO
    {
        public int ProjectId { get; set; }
        public int StatusId { get; set; }
        public int? ResponsibleId { get; set; }
        public int CreatedById { get; set; }
        public InfoCreateDTO Info { get; set; } = new InfoCreateDTO();
    }

    public class DefectUpdateDTO
    {
        public int ProjectId { get; set; }
        public int StatusId { get; set; }
        public int? ResponsibleId { get; set; }
        public InfoUpdateDTO Info { get; set; } = new InfoUpdateDTO();
    }
}