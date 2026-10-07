using ClassAssistant.Backend.Models;

namespace ClassAssistant.Backend.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        try
        {
            context.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbInitializer WARNING] Migration/EnsureCreated: {ex.Message}");
        }

        if (!context.Teachers.Any(t => t.Role == "admin"))
        {
            var adminUser = new Teacher
            {
                Name = "System Administrator",
                Email = "admin@green.edu.bd",
                Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "admin",
                Courses = "CSE-101, CSE-201"
            };
            context.Teachers.Add(adminUser);
        }

        if (!context.Teachers.Any(t => t.Role == "user" || t.Role == "faculty"))
        {
            var facultyUser = new Teacher
            {
                Name = "Faculty Member",
                Email = "teacher@green.edu.bd",
                Password = BCrypt.Net.BCrypt.HashPassword("teacher123"),
                Role = "user",
                Courses = "CSE-101, CSE-102"
            };
            context.Teachers.Add(facultyUser);
        }

        if (!context.Courses.Any())
        {
            var defaultCourses = new List<Course>
            {
                new Course { Code = "CSE-101", Title = "Structured Programming", Name = "Structured Programming", Section = "A" },
                new Course { Code = "CSE-102", Title = "Data Structures & Algorithms", Name = "Data Structures & Algorithms", Section = "A" },
                new Course { Code = "CSE-201", Title = "Object Oriented Programming", Name = "Object Oriented Programming", Section = "B" },
                new Course { Code = "MAT-101", Title = "Linear Algebra & Differential Equations", Name = "Linear Algebra & Differential Equations", Section = "A" },
                new Course { Code = "EEE-101", Title = "Electrical Circuits & Electronics", Name = "Electrical Circuits & Electronics", Section = "A" },
                new Course { Code = "ENG-101", Title = "English Communication Skills", Name = "English Communication Skills", Section = "A" }
            };
            context.Courses.AddRange(defaultCourses);
        }

        if (!context.Students.Any())
        {
            var studentUser = new Student
            {
                Id = 221902117,
                Name = "John Doe",
                Password = BCrypt.Net.BCrypt.HashPassword("student123"),
                Courses = "CSE-101, CSE-102, CSE-201"
            };
            context.Students.Add(studentUser);
        }

        if (!context.Settings.Any())
        {
            context.Settings.Add(new Setting
            {
                Id = 1,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(17, 0, 0),
                LateCount = 15
            });
        }

        context.SaveChanges();
    }
}
