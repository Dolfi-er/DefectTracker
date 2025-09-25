namespace Back.Models.DTOs
{
    public class CommentDto
    {
        public int CommentId { get; set; }
        public int DefectId { get; set; }
        public UserDto User { get; set; } = new UserDto();
        public string CommentText { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
    
    public class CreateCommentDto
    {
        public int DefectId { get; set; }
        public string CommentText { get; set; } = string.Empty;
    }
    
    public class UpdateCommentDto
    {
        public string CommentText { get; set; } = string.Empty;
    }
}