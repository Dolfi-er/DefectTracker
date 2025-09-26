namespace Back.DTOs
{
    public class DefectAttachmentDTO
    {
        public int AttachmentId { get; set; }
        public int DefectId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public int UploadedById { get; set; }
        public string? UploadedByFio { get; set; }
    }

    public class DefectAttachmentCreateDTO
    {
        public int DefectId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int UploadedById { get; set; }
    }

    public class DefectAttachmentUpdateDTO
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}