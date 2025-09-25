namespace Back.Models.DTOs
{
    public class DefectDto
    {
        public int DefectId { get; set; }
        public ProjectDto Project { get; set; } = new ProjectDto();
        public DefectStatusDto Status { get; set; } = new DefectStatusDto();
        public DefectInfoDto Info { get; set; } = new DefectInfoDto();
        public UserDto? Responsible { get; set; }
        public UserDto CreatedBy { get; set; } = new UserDto();
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int CommentCount { get; set; }
        public int AttachmentCount { get; set; }
    }
    
    public class CreateDefectDto
    {
        public int ProjectId { get; set; }
        public string DefectName { get; set; } = string.Empty;
        public string DefectDescription { get; set; } = string.Empty;
        public short Priority { get; set; } = 1;
        public DateTime DueDate { get; set; }
        public int? ResponsibleId { get; set; }
    }
    
    public class UpdateDefectDto
    {
        public string DefectName { get; set; } = string.Empty;
        public string DefectDescription { get; set; } = string.Empty;
        public short Priority { get; set; }
        public DateTime DueDate { get; set; }
        public int? ResponsibleId { get; set; }
        public int StatusId { get; set; }
    }
    
    public class DefectStatusDto
    {
        public int Id { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string StatusDescription { get; set; } = string.Empty;
    }
    
    public class DefectInfoDto
    {
        public int InfoId { get; set; }
        public string DefectName { get; set; } = string.Empty;
        public string DefectDescription { get; set; } = string.Empty;
        public short Priority { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime StatusChangeDate { get; set; }
    }
}