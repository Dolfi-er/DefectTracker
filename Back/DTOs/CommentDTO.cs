namespace Back.DTOs
{
    public class CommentDTO
    {
        public int CommentId { get; set; }
        public int DefectId { get; set; }
        public int UserId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string? UserFio { get; set; }
    }

    public class CommentCreateDTO
    {
        public int DefectId { get; set; }
        public int UserId { get; set; }
        public string CommentText { get; set; } = string.Empty;
    }

    public class CommentUpdateDTO
    {
        public string CommentText { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
    }
}