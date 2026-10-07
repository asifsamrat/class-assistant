using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ClassAssistant.Backend.Models;

[Table("students")]
public class Student
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("password")]
    [JsonIgnore]
    public string? Password { get; set; }

    [MaxLength(500)]
    [Column("courses")]
    public string Courses { get; set; } = string.Empty;

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
