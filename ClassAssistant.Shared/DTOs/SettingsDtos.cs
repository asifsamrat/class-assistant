namespace ClassAssistant.Shared.DTOs;

public class SettingsDto
{
    public int Id { get; set; } = 1;
    public string Start { get; set; } = "09:00";
    public string End { get; set; } = "17:00";
    public int Late { get; set; } = 15;
}

public class UpdateSettingsRequest
{
    public int Id { get; set; } = 1;
    public string StartTime { get; set; } = "09:00:00 AM";
    public string EndTime { get; set; } = "05:00:00 PM";
    public int LateCount { get; set; } = 15;
}
