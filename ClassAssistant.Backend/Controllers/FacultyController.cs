using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClassAssistant.Backend.Data;
using ClassAssistant.Backend.Models;
using ClassAssistant.Backend.Services;
using ClassAssistant.Shared.DTOs;

namespace ClassAssistant.Backend.Controllers;

[ApiController]
[Route("faculty")]
public class FacultyController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private const string CookieName = "attendance-system";

    public FacultyController(AppDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpGet("my_courses")]
    public async Task<IActionResult> GetMyCourses()
    {
        var allCourses = await _context.Courses.ToListAsync();
        var allCoursesDict = allCourses.Select(c => new CourseDto
        {
            Id = c.Id,
            Code = c.Code,
            Title = string.IsNullOrWhiteSpace(c.Title) ? c.Name : c.Title,
            Section = c.Section
        }).ToList();

        string? token = Request.Cookies[CookieName];
        var principal = !string.IsNullOrEmpty(token) ? _jwtService.ValidateToken(token) : null;
        if (principal == null) return Ok(allCoursesDict);

        string email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
        string userName = principal.FindFirst("user")?.Value ?? principal.Identity?.Name ?? "";

        Teacher? teacher = null;
        if (!string.IsNullOrEmpty(email))
        {
            teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Email.ToLower() == email.ToLower());
        }
        teacher ??= await _context.Teachers.FirstOrDefaultAsync(t => t.Name == userName);

        if (teacher == null || string.IsNullOrWhiteSpace(teacher.Courses))
            return Ok(allCoursesDict);

        var assignedRaw = teacher.Courses.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim().ToUpper()).ToList();

        if (assignedRaw.Count == 0) return Ok(allCoursesDict);

        var assignedCourses = allCoursesDict.Where(c =>
            assignedRaw.Any(a => a == c.Code.ToUpper() || c.Code.ToUpper().Contains(a) ||
                                 a == c.Title.ToUpper() || c.Title.ToUpper().Contains(a) ||
                                 a == c.Id.ToString())
        ).ToList();

        return Ok(assignedCourses.Count > 0 ? assignedCourses : allCoursesDict);
    }

    [HttpGet("absence_notices")]
    public async Task<IActionResult> GetAbsenceNotices()
    {
        var notices = await _context.AbsenceNotices
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

        return Ok(notices);
    }

    [HttpPut("absence_notices/{noticeId}")]
    public async Task<IActionResult> UpdateAbsenceNoticeStatus(int noticeId, [FromBody] UpdateNoticeStatusRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Status)) return BadRequest(new { msg = "Status is required" });

        var notice = await _context.AbsenceNotices.FindAsync(noticeId);
        if (notice == null) return NotFound(new { msg = "Absence notice not found" });

        notice.Status = req.Status.Trim();
        await _context.SaveChangesAsync();

        DateTime noticeDate = DateTime.UtcNow.Date;
        if (DateTime.TryParse(notice.Date, out var pDate)) noticeDate = pDate.Date;

        string noticeCourse = (notice.Course ?? "").Trim().ToUpper();

        var existingAtt = await _context.Attendances.FirstOrDefaultAsync(a =>
            a.StudentId == notice.StudentId &&
            a.Date.Date == noticeDate &&
            (string.IsNullOrEmpty(noticeCourse) || a.CourseCode.ToUpper() == noticeCourse));

        if (req.Status.Trim().Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            if (existingAtt == null)
            {
                var newAtt = new Attendance
                {
                    StudentId = notice.StudentId,
                    CourseCode = noticeCourse,
                    Date = noticeDate,
                    EntryTime = DateTime.UtcNow,
                    LastSeenTime = DateTime.UtcNow,
                    TotalMinutes = 60,
                    IsPresent = 1.0
                };
                _context.Attendances.Add(newAtt);
            }
            else
            {
                existingAtt.IsPresent = 1.0;
                if (!string.IsNullOrEmpty(noticeCourse)) existingAtt.CourseCode = noticeCourse;
            }
            await _context.SaveChangesAsync();
        }
        else if (req.Status.Trim().Equals("Rejected", StringComparison.OrdinalIgnoreCase))
        {
            if (existingAtt != null)
            {
                existingAtt.IsPresent = 0.0;
                await _context.SaveChangesAsync();
            }
        }

        return Ok(new
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
}
