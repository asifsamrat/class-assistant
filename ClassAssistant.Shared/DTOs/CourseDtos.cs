namespace ClassAssistant.Shared.DTOs;

public class CourseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Section { get; set; } = "A";
}

public class CourseSaveRequest
{
    public int? Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Section { get; set; } = "A";
}
