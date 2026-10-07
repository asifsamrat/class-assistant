namespace ClassAssistant.Shared.DTOs;

public class AbsenceNoticeDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Submitted";
    public string CreatedAt { get; set; } = string.Empty;
}

public class SubmitAbsenceNoticeRequest
{
    public string Course { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class UpdateNoticeStatusRequest
{
    public string Status { get; set; } = "Submitted"; // "Approved" or "Rejected"
}
