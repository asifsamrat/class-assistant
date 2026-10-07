using Microsoft.EntityFrameworkCore;
using ClassAssistant.Backend.Models;

namespace ClassAssistant.Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<VideoFeed> VideoFeeds => Set<VideoFeed>();
    public DbSet<AbsenceNotice> AbsenceNotices => Set<AbsenceNotice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasIndex(t => t.Email).IsUnique();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasIndex(s => s.Name);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasIndex(c => c.Code);
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasOne(a => a.Student)
                  .WithMany(s => s.Attendances)
                  .HasForeignKey(a => a.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
