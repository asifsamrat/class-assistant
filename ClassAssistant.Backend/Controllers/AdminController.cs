using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClassAssistant.Backend.Data;
using ClassAssistant.Backend.Models;
using ClassAssistant.Shared.DTOs;

namespace ClassAssistant.Backend.Controllers;

[ApiController]
[Route("admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents()
    {
        var students = await _context.Students.ToListAsync();
        var result = students.Select(s => new StudentDto
        {
            Id = s.Id,
            Name = s.Name,
            Courses = s.Courses ?? "",
            HasPassword = !string.IsNullOrEmpty(s.Password)
        });
        return Ok(result);
    }

    [HttpPost("students")]
    public async Task<IActionResult> SaveStudent([FromBody] StudentSaveRequest req)
    {
        if (req.Id <= 0 || string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { msg = "Student ID and Name are required" });

        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == (req.OriginalId ?? req.Id));
        if (student == null)
        {
            student = new Student
            {
                Id = req.Id,
                Name = req.Name.Trim(),
                Courses = req.Courses ?? ""
            };
            if (!string.IsNullOrWhiteSpace(req.Password))
            {
                student.Password = BCrypt.Net.BCrypt.HashPassword(req.Password.Trim());
            }
            _context.Students.Add(student);
        }
        else
        {
            student.Name = req.Name.Trim();
            student.Courses = req.Courses ?? "";
            if (!string.IsNullOrWhiteSpace(req.Password))
            {
                student.Password = BCrypt.Net.BCrypt.HashPassword(req.Password.Trim());
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { msg = "success", student = new StudentDto { Id = student.Id, Name = student.Name, Courses = student.Courses } });
    }

    [HttpDelete("students/{studentId}")]
    public async Task<IActionResult> DeleteStudent(int studentId)
    {
        var student = await _context.Students.FindAsync(studentId);
        if (student == null) return NotFound(new { msg = "Student not found" });

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
        return Ok(new { msg = "success" });
    }

    [HttpGet("faculty")]
    public async Task<IActionResult> GetFaculty()
    {
        var faculty = await _context.Teachers.ToListAsync();
        var result = faculty.Select(f => new FacultyDto
        {
            Id = f.Id,
            Name = f.Name,
            Email = f.Email,
            Role = f.Role,
            Courses = f.Courses ?? ""
        });
        return Ok(result);
    }

    [HttpPost("faculty")]
    public async Task<IActionResult> SaveFaculty([FromBody] FacultySaveRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Email))
            return BadRequest(new { msg = "Name and Email are required" });

        Teacher? teacher = null;
        if (req.Id.HasValue && req.Id.Value > 0)
        {
            teacher = await _context.Teachers.FindAsync(req.Id.Value);
        }

        teacher ??= await _context.Teachers.FirstOrDefaultAsync(t => t.Email.ToLower() == req.Email.Trim().ToLower());

        if (teacher == null)
        {
            if (string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { msg = "Password is required for new faculty account" });

            teacher = new Teacher
            {
                Name = req.Name.Trim(),
                Email = req.Email.Trim().ToLower(),
                Password = BCrypt.Net.BCrypt.HashPassword(req.Password.Trim()),
                Role = string.IsNullOrWhiteSpace(req.Role) ? "user" : req.Role,
                Courses = req.Courses ?? ""
            };
            _context.Teachers.Add(teacher);
        }
        else
        {
            teacher.Name = req.Name.Trim();
            teacher.Email = req.Email.Trim().ToLower();
            teacher.Role = string.IsNullOrWhiteSpace(req.Role) ? "user" : req.Role;
            teacher.Courses = req.Courses ?? "";
            if (!string.IsNullOrWhiteSpace(req.Password))
            {
                teacher.Password = BCrypt.Net.BCrypt.HashPassword(req.Password.Trim());
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { msg = "success", faculty = new FacultyDto { Id = teacher.Id, Name = teacher.Name, Email = teacher.Email, Role = teacher.Role, Courses = teacher.Courses } });
    }

    [HttpDelete("faculty/{facultyId}")]
    public async Task<IActionResult> DeleteFaculty(int facultyId)
    {
        var teacher = await _context.Teachers.FindAsync(facultyId);
        if (teacher == null) return NotFound(new { msg = "Faculty not found" });

        _context.Teachers.Remove(teacher);
        await _context.SaveChangesAsync();
        return Ok(new { msg = "success" });
    }

    [HttpGet("courses")]
    public async Task<IActionResult> GetCourses()
    {
        var courses = await _context.Courses.ToListAsync();
        var result = courses.Select(c => new CourseDto
        {
            Id = c.Id,
            Code = c.Code,
            Title = string.IsNullOrWhiteSpace(c.Title) ? c.Name : c.Title,
            Section = c.Section
        });
        return Ok(result);
    }

    [HttpPost("courses")]
    public async Task<IActionResult> SaveCourse([FromBody] CourseSaveRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Code) || string.IsNullOrWhiteSpace(req.Title))
            return BadRequest(new { msg = "Course code and title are required" });

        string codeUpper = req.Code.Trim().ToUpper();
        Course? course = null;

        if (req.Id.HasValue && req.Id.Value > 0)
        {
            course = await _context.Courses.FindAsync(req.Id.Value);
        }

        course ??= await _context.Courses.FirstOrDefaultAsync(c => c.Code.ToUpper() == codeUpper);

        if (course == null)
        {
            course = new Course
            {
                Code = codeUpper,
                Title = req.Title.Trim(),
                Name = req.Title.Trim(),
                Section = string.IsNullOrWhiteSpace(req.Section) ? "A" : req.Section
            };
            _context.Courses.Add(course);
        }
        else
        {
            course.Code = codeUpper;
            course.Title = req.Title.Trim();
            course.Name = req.Title.Trim();
            course.Section = string.IsNullOrWhiteSpace(req.Section) ? "A" : req.Section;
        }

        await _context.SaveChangesAsync();
        return Ok(new { msg = "success", course = new CourseDto { Id = course.Id, Code = course.Code, Title = course.Title, Section = course.Section } });
    }

    [HttpDelete("courses/{courseId}")]
    public async Task<IActionResult> DeleteCourse(int courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return NotFound(new { msg = "Course not found" });

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return Ok(new { msg = "success" });
    }
}
