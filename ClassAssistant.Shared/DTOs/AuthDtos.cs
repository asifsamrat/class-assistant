namespace ClassAssistant.Shared.DTOs;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Msg { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? StudentId { get; set; }
}

public class StaySigninResponse
{
    public string Msg { get; set; } = "success";
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "faculty";
    public string Courses { get; set; } = string.Empty;
    public int? StudentId { get; set; }
}
