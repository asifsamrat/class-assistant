namespace ClassAssistant.Shared.DTOs;

public class StudentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Courses { get; set; } = string.Empty;
    public bool HasPassword { get; set; }
}

public class StudentSaveRequest
{
    public int Id { get; set; }
    public int? OriginalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Courses { get; set; } = string.Empty;
}

public class StudentPortalDataResponse
{
    public string Msg { get; set; } = "success";
    public StudentProfileDto Student { get; set; } = new();
    public List<CourseStatDto> CourseStats { get; set; } = new();
    public List<AttendanceLogDto> AttendanceLogs { get; set; } = new();
    public List<AbsenceNoticeDto> AbsenceNotices { get; set; } = new();
}

public class StudentProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Courses { get; set; } = new();
    public bool IsFaceRegistered { get; set; }
    public int RegisteredSamplesCount { get; set; }
}

public class CourseStatDto
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int TotalClasses { get; set; }
    public int AttendedClasses { get; set; }
    public int AbsentClasses { get; set; }
    public double Percentage { get; set; }
    public string Status { get; set; } = "Good Standing";
}

public class AttendanceLogDto
{
    public int Id { get; set; }
    public string Course { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string CheckIn { get; set; } = string.Empty;
    public string CheckOut { get; set; } = string.Empty;
    public int TotalMinutes { get; set; }
    public string Status { get; set; } = "Present";
}
