using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassAssistant.Backend.Models;

[Table("video_feeds")]
public class VideoFeed
{
    [Key]
    [MaxLength(30)]
    [Column("id")]
    public string Id { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = false;

    [Required]
    [Column("url")]
    public string Url { get; set; } = string.Empty;
}
