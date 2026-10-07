using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassAssistant.Backend.Models;

[Table("settings")]
public class Setting
{
    [Key]
    [Column("id")]
    public int Id { get; set; } = 1;

    [Column("start_time")]
    public TimeSpan StartTime { get; set; } = new TimeSpan(9, 0, 0);

    [Column("end_time")]
    public TimeSpan EndTime { get; set; } = new TimeSpan(17, 0, 0);

    [Column("late_count")]
    public int LateCount { get; set; } = 15;
}
