namespace ClassAssistant.Shared.DTOs;

public class FacultyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public string Courses { get; set; } = string.Empty;
}

public class FacultySaveRequest
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public string Courses { get; set; } = string.Empty;
}
