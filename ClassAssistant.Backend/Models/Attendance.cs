using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassAssistant.Backend.Models;

[Table("attendances")]
public class Attendance
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("date")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Column("student_id")]
    public int StudentId { get; set; }

    [ForeignKey("StudentId")]
    public virtual Student? Student { get; set; }

    [MaxLength(50)]
    [Column("course_code")]
    public string CourseCode { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("session_id")]
    public string SessionId { get; set; } = string.Empty;

    [Column("entry_time")]
    public DateTime EntryTime { get; set; } = DateTime.UtcNow;

    [Column("last_seen_time")]
    public DateTime LastSeenTime { get; set; } = DateTime.UtcNow;

    [Column("total_minutes")]
    public int TotalMinutes { get; set; } = 0;

    [Column("is_present")]
    public double IsPresent { get; set; } = 0.0;
}
