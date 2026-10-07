using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassAssistant.Backend.Models;

[Table("courses")]
public class Course
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("section")]
    public string Section { get; set; } = "A";

    [MaxLength(120)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("is_open_credit")]
    public bool IsOpenCredit { get; set; } = false;
}
