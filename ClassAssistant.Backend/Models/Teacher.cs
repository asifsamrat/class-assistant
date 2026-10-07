using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ClassAssistant.Backend.Models;

[Table("teachers")]
public class Teacher
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("password")]
    [JsonIgnore]
    public string Password { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("role")]
    public string Role { get; set; } = "user";

    [MaxLength(500)]
    [Column("courses")]
    public string Courses { get; set; } = string.Empty;
}
