using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using ClassAssistant.Backend.Services;

namespace ClassAssistant.Backend.Hubs;

public class SessionInfo
{
    public string CourseCode { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string SelectedDate { get; set; } = string.Empty;
}

public class AttendanceHub : Hub
{
    private static readonly ConcurrentDictionary<string, SessionInfo> ConnectionSessions = new();
    private readonly IFaceRecognitionService _faceRecognitionService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IWebHostEnvironment _env;

    public AttendanceHub(
        IFaceRecognitionService faceRecognitionService,
        IServiceScopeFactory scopeFactory,
        IWebHostEnvironment env)
    {
        _faceRecognitionService = faceRecognitionService;
        _scopeFactory = scopeFactory;
        _env = env;
    }

    public async Task SetActiveCourse(object data)
    {
        string courseCode = string.Empty;
        string sessionId = string.Empty;
        string selectedDate = string.Empty;

        if (data is System.Text.Json.JsonElement elem && elem.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            if (elem.TryGetProperty("course_code", out var cProp) || elem.TryGetProperty("code", out cProp))
                courseCode = cProp.GetString() ?? string.Empty;

            if (elem.TryGetProperty("session_id", out var sProp))
                sessionId = sProp.GetString() ?? string.Empty;

            if (elem.TryGetProperty("selected_date", out var dProp) || elem.TryGetProperty("date", out dProp))
                selectedDate = dProp.GetString() ?? string.Empty;
        }

        ConnectionSessions[Context.ConnectionId] = new SessionInfo
        {
            CourseCode = courseCode.Trim().ToUpper(),
            SessionId = sessionId.Trim(),
            SelectedDate = selectedDate.Trim()
        };

        await Task.CompletedTask;
    }

    public async Task SendClientFrame(byte[] frameData)
    {
        if (frameData == null || frameData.Length == 0) return;

        ConnectionSessions.TryGetValue(Context.ConnectionId, out var sess);
        sess ??= new SessionInfo();

        var (processedFrame, names) = _faceRecognitionService.ProcessFrame(
            frameData,
            sess.CourseCode,
            sess.SessionId,
            sess.SelectedDate,
            _scopeFactory);

        if (processedFrame != null && processedFrame.Length > 0)
        {
            await Clients.Caller.SendAsync("processed_frame", processedFrame);
        }
    }

    public async Task RegisterCaptureFrame(int studentId, byte[] frameData)
    {
        if (frameData == null || frameData.Length == 0) return;

        string datasetPath = Path.Combine(_env.ContentRootPath, "files", "dataset");
        var (processedFrame, capturedCount, isComplete) = _faceRecognitionService.ProcessRegistrationFrame(
            studentId,
            frameData,
            datasetPath);

        if (processedFrame != null && processedFrame.Length > 0)
        {
            await Clients.Caller.SendAsync("register_processed_frame", processedFrame);
        }

        await Clients.Caller.SendAsync("register_progress", new
        {
            student_id = studentId,
            captured_count = capturedCount,
            target = 15,
            status = isComplete ? "complete" : "capturing"
        });

        if (isComplete)
        {
            await Clients.Caller.SendAsync("register_complete", new
            {
                student_id = studentId,
                message = "Student face registered and model trained successfully!"
            });
        }
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        ConnectionSessions.TryRemove(Context.ConnectionId, out _);
        return base.OnDisconnectedAsync(exception);
    }
}
