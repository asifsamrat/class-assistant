using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassAssistant.Backend.Models;

[Table("absence_notices")]
public class AbsenceNotice
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("student_name")]
    public string StudentName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("course")]
    public string Course { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("date")]
    public string Date { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    [Column("reason")]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(30)]
    [Column("status")]
    public string Status { get; set; } = "Submitted";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
