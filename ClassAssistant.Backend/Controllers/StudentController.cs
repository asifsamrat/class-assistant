using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClassAssistant.Backend.Data;
using ClassAssistant.Backend.Models;
using ClassAssistant.Backend.Services;
using ClassAssistant.Shared.DTOs;

namespace ClassAssistant.Backend.Controllers;

[ApiController]
public class StudentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IWebHostEnvironment _env;
    private const string CookieName = "attendance-system";

    public StudentController(AppDbContext context, IJwtService jwtService, IWebHostEnvironment env)
    {
        _context = context;
        _jwtService = jwtService;
        _env = env;
    }

    [HttpGet("student/portal_data")]
    public async Task<IActionResult> GetStudentPortalData()
    {
        string? token = Request.Cookies[CookieName];
        var principal = !string.IsNullOrEmpty(token) ? _jwtService.ValidateToken(token) : null;

        int? studentId = null;
        string? name = null;

        if (principal != null)
        {
            var idClaim = principal.FindFirst("student_id")?.Value;
            if (int.TryParse(idClaim, out int val)) studentId = val;
            name = principal.FindFirst("user")?.Value ?? principal.Identity?.Name;
        }

        Student? student = null;
        if (studentId.HasValue)
        {
            student = await _context.Students.Include(s => s.Attendances).FirstOrDefaultAsync(s => s.Id == studentId.Value);
        }
        if (student == null && !string.IsNullOrEmpty(name))
        {
            student = await _context.Students.Include(s => s.Attendances).FirstOrDefaultAsync(s => s.Name == name);
        }
        student ??= await _context.Students.Include(s => s.Attendances).FirstOrDefaultAsync();

        if (student == null) return NotFound(new { msg = "Student profile not found" });

        var assignedCoursesRaw = (student.Courses ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()).ToList();
        var allCourses = await _context.Courses.ToListAsync();

        if (assignedCoursesRaw.Count == 0)
        {
            assignedCoursesRaw = allCourses.Select(c => c.Code).ToList();
        }

        var allAttendances = await _context.Attendances.ToListAsync();
        var studentAttendances = allAttendances.Where(a => a.StudentId == student.Id).ToList();

        var courseStats = new List<CourseStatDto>();

        foreach (var cCode in assignedCoursesRaw)
        {
            var cModel = allCourses.FirstOrDefault(c => c.Code.Equals(cCode, StringComparison.OrdinalIgnoreCase));
            string cTitle = cModel != null && !string.IsNullOrEmpty(cModel.Title) ? cModel.Title : (cModel?.Name ?? cCode);

            string cCodeClean = cCode.Trim().ToUpper();

            var courseAllAtts = allAttendances.Where(a =>
                !string.IsNullOrEmpty(a.CourseCode) &&
                a.CourseCode.Trim().ToUpper() == cCodeClean).ToList();

            var distinctDates = courseAllAtts.Select(a => a.Date.Date).Distinct().ToList();
            int totalClasses = Math.Max(distinctDates.Count, courseAllAtts.Count > 0 ? 1 : 0);

            var studentCourseAtts = studentAttendances.Where(a =>
                a.IsPresent > 0 &&
                !string.IsNullOrEmpty(a.CourseCode) &&
                a.CourseCode.Trim().ToUpper() == cCodeClean).ToList();

            int attendedCount = studentCourseAtts.Count;
            int absentCount = Math.Max(0, totalClasses - attendedCount);

            double percentage = totalClasses > 0 ? Math.Round(((double)attendedCount / totalClasses) * 100, 1) : 100.0;
            string status = percentage >= 80 ? "Good Standing" : percentage >= 70 ? "Warning" : "Low Attendance";

            courseStats.Add(new CourseStatDto
            {
                Code = cCode,
                Title = cTitle,
                TotalClasses = totalClasses,
                AttendedClasses = attendedCount,
                AbsentClasses = absentCount,
                Percentage = percentage,
                Status = status
            });
        }

        var attendanceLogs = studentAttendances.Select(a => new AttendanceLogDto
        {
            Id = a.Id,
            Course = string.IsNullOrEmpty(a.CourseCode) ? "CSE-101" : a.CourseCode,
            Date = a.Date.ToString("yyyy-MM-dd"),
            CheckIn = a.EntryTime.ToString("hh:mm tt"),
            CheckOut = a.LastSeenTime.ToString("hh:mm tt"),
            TotalMinutes = a.TotalMinutes,
            Status = a.IsPresent > 0 ? "Present" : "Absent"
        }).ToList();

        var notices = await _context.AbsenceNotices
            .Where(n => n.StudentId == student.Id)
            .OrderByDescending(n => n.Id)
            .Select(n => new AbsenceNoticeDto
            {
                Id = n.Id,
                StudentId = n.StudentId,
                StudentName = n.StudentName,
                Course = n.Course ?? string.Empty,
                Date = n.Date ?? string.Empty,
                Reason = n.Reason ?? string.Empty,
                Status = n.Status ?? string.Empty,
                CreatedAt = n.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToListAsync();

        string datasetPath = Path.Combine(_env.ContentRootPath, "files", "dataset", student.Id.ToString());
        int samplesCount = Directory.Exists(datasetPath) ? Directory.GetFiles(datasetPath, "*.jpg").Length : 0;
        bool isFaceRegistered = samplesCount >= 10;

        return Ok(new StudentPortalDataResponse
        {
            Msg = "success",
            Student = new StudentProfileDto
            {
                Id = student.Id,
                Name = student.Name,
                Courses = assignedCoursesRaw,
                IsFaceRegistered = isFaceRegistered,
                RegisteredSamplesCount = samplesCount
            },
            CourseStats = courseStats,
            AttendanceLogs = attendanceLogs,
            AbsenceNotices = notices
        });
    }

    [HttpPost("student/absence_notice")]
    public async Task<IActionResult> SubmitAbsenceNotice([FromBody] SubmitAbsenceNoticeRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Course) || string.IsNullOrWhiteSpace(req.Date) || string.IsNullOrWhiteSpace(req.Reason))
            return BadRequest(new { msg = "Course, Date, and Reason are required" });

        string? token = Request.Cookies[CookieName];
        var principal = !string.IsNullOrEmpty(token) ? _jwtService.ValidateToken(token) : null;

        int studentId = 1;
        string studentName = "Student";

        if (principal != null)
        {
            var idClaim = principal.FindFirst("student_id")?.Value;
            if (int.TryParse(idClaim, out int val)) studentId = val;
            studentName = principal.FindFirst("user")?.Value ?? principal.Identity?.Name ?? "Student";
        }

        var student = await _context.Students.FindAsync(studentId);
        if (student != null)
        {
            studentName = student.Name;
        }

        var notice = new AbsenceNotice
        {
            StudentId = studentId,
            StudentName = studentName,
            Course = req.Course.Trim(),
            Date = req.Date.Trim(),
            Reason = req.Reason.Trim(),
            Status = "Submitted",
            CreatedAt = DateTime.UtcNow
        };

        _context.AbsenceNotices.Add(notice);
        await _context.SaveChangesAsync();

        return StatusCode(201, new
        {
            msg = "success",
            notice = new AbsenceNoticeDto
            {
                Id = notice.Id,
                StudentId = notice.StudentId,
                StudentName = notice.StudentName,
                Course = notice.Course ?? string.Empty,
                Date = notice.Date ?? string.Empty,
                Reason = notice.Reason ?? string.Empty,
                Status = notice.Status ?? string.Empty,
                CreatedAt = notice.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            }
        });
    }

    [HttpPost("register_student")]
    public async Task<IActionResult> RegisterStudent([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest(new { error = "Student name is required" });

        var student = new Student
        {
            Id = int.TryParse(req.StudentId, out var idVal) ? idVal : Random.Shared.Next(100000, 999999),
            Name = req.Name.Trim()
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        string datasetPath = Path.Combine(_env.ContentRootPath, "files", "dataset", student.Id.ToString());
        if (!Directory.Exists(datasetPath)) Directory.CreateDirectory(datasetPath);

        return StatusCode(201, new { id = student.Id, name = student.Name, message = "Student created successfully" });
    }

    [HttpPost("student/start_face_registration")]
    public async Task<IActionResult> StartFaceRegistration()
    {
        string? token = Request.Cookies[CookieName];
        var principal = !string.IsNullOrEmpty(token) ? _jwtService.ValidateToken(token) : null;

        int studentId = 1;
        string name = "Student";

        if (principal != null)
        {
            var idClaim = principal.FindFirst("student_id")?.Value;
            if (int.TryParse(idClaim, out int val)) studentId = val;
            name = principal.FindFirst("user")?.Value ?? principal.Identity?.Name ?? "Student";
        }

        var student = await _context.Students.FindAsync(studentId);
        if (student != null)
        {
            name = student.Name;
        }

        string datasetPath = Path.Combine(_env.ContentRootPath, "files", "dataset", studentId.ToString());
        if (!Directory.Exists(datasetPath)) Directory.CreateDirectory(datasetPath);

        return Ok(new { msg = "success", student_id = studentId, name });
    }
}
