namespace Back.DTOs
{
    public class DefectHistoryDTO
    {
        public int HistoryId { get; set; }
        public int DefectId { get; set; }
        public int UserId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public DateTime ChangeDate { get; set; }
        public string? UserFio { get; set; }
    }
}