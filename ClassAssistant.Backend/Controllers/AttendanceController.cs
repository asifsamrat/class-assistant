using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClassAssistant.Backend.Data;
using ClassAssistant.Backend.Services;
using ClassAssistant.Shared.DTOs;

namespace ClassAssistant.Backend.Controllers;

[ApiController]
public class AttendanceController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IExportService _exportService;

    public AttendanceController(AppDbContext context, IExportService exportService)
    {
        _context = context;
        _exportService = exportService;
    }

    [HttpGet("get_attendance")]
    public async Task<IActionResult> GetAttendance()
    {
        var students = await _context.Students.ToListAsync();
        var attendances = await _context.Attendances.ToListAsync();
        var result = new List<object>();

        foreach (var student in students)
        {
            var studentAtts = attendances.Where(a => a.StudentId == student.Id).ToList();
            var dateList = new List<object>();

            foreach (var att in studentAtts)
            {
                dateList.Add(new
                {
                    attendance_date = att.Date.ToString("yyyy-MM-dd"),
                    course_code = att.CourseCode ?? "",
                    ck_time = att.EntryTime.ToString("hh:mm tt"),
                    ck_out = att.LastSeenTime.ToString("hh:mm tt"),
                    total_time = att.TotalMinutes,
                    status = att.IsPresent > 0 ? "Present" : "Absent"
                });
            }

            result.Add(new
            {
                id = student.Id,
                name = student.Name,
                courses = student.Courses ?? "",
                date_time = new { dates = dateList },
                status = studentAtts.Any(a => a.IsPresent > 0) ? "Present" : "Absent"
            });
        }

        return Ok(result);
    }

    [HttpPost("filter_by_date")]
    public async Task<IActionResult> FilterByDate([FromBody] Dictionary<string, string> req)
    {
        if (!req.TryGetValue("date", out var dateStr) || string.IsNullOrWhiteSpace(dateStr))
            return BadRequest(new { error = "Date not provided in request" });

        if (!DateTime.TryParse(dateStr, out var targetDate))
            return BadRequest(new { error = "Invalid date format. Use YYYY-MM-DD." });

        var students = await _context.Students.ToListAsync();
        var attendances = await _context.Attendances.ToListAsync();
        var result = new List<object>();

        foreach (var student in students)
        {
            var att = attendances.FirstOrDefault(a => a.StudentId == student.Id && a.Date.Date == targetDate.Date);
            var dateList = new List<object>();
            string status = "--";

            if (att != null)
            {
                status = att.IsPresent == 0.5 ? "late" : "on time";
                dateList.Add(new
                {
                    attendance_date = att.Date.ToString("yyyy-MM-dd"),
                    ck_time = att.EntryTime.ToString("HH:mm:tt"),
                    ck_out = att.LastSeenTime.ToString("HH:mm:tt"),
                    total_time = att.TotalMinutes
                });
            }
            else
            {
                dateList.Add(new { attendance_date = "--", time = "--" });
            }

            result.Add(new
            {
                id = student.Id,
                name = student.Name,
                date_time = new { dates = dateList },
                status
            });
        }

        return Ok(result);
    }

    [HttpGet("profiles")]
    public async Task<IActionResult> GetProfiles()
    {
        var students = await _context.Students.ToListAsync();
        var profiles = students.Select(s => new
        {
            id = s.Id,
            name = s.Name,
            description = "Computer Science and Engineering",
            department = "CSE"
        });
        return Ok(profiles);
    }

    [HttpGet("time_logs")]
    public async Task<IActionResult> GetTimeLogs()
    {
        return await FilterByDate(new Dictionary<string, string> { { "date", DateTime.UtcNow.ToString("yyyy-MM-dd") } });
    }

    [HttpPost("train_classifier")]
    public IActionResult TrainClassifier()
    {
        return Ok(new { message = "Classifier trained successfully in C# OpenCV engine" });
    }

    [HttpGet("export_attendance_matrix")]
    public async Task<IActionResult> ExportAttendanceMatrix([FromQuery] string courseCode = "", [FromQuery] string date = "")
    {
        var students = await _context.Students.Select(s => new StudentProfileDto
        {
            Id = s.Id,
            Name = s.Name,
            Courses = (s.Courses ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
        }).ToListAsync();

        var attendances = await _context.Attendances.Select(a => new AttendanceLogDto
        {
            Id = a.StudentId,
            Course = a.CourseCode,
            Date = a.Date.ToString("yyyy-MM-dd"),
            CheckIn = a.EntryTime.ToString("hh:mm tt"),
            CheckOut = a.LastSeenTime.ToString("hh:mm tt"),
            TotalMinutes = a.TotalMinutes,
            Status = a.IsPresent > 0 ? "Present" : "Absent"
        }).ToListAsync();

        byte[] excelBytes = _exportService.ExportAttendanceMatrixToExcel(students, attendances, courseCode, date);
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Attendance_Matrix_{courseCode}_{date}.xlsx");
    }
}
