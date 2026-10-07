using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClassAssistant.Backend.Data;
using ClassAssistant.Backend.Models;
using ClassAssistant.Shared.DTOs;

namespace ClassAssistant.Backend.Controllers;

[ApiController]
[Route("settings")]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SettingsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Id == 1);
        setting ??= new Setting();

        return Ok(new SettingsDto
        {
            Id = setting.Id,
            Start = $"{setting.StartTime.Hours:D2}:{setting.StartTime.Minutes:D2}",
            End = $"{setting.EndTime.Hours:D2}:{setting.EndTime.Minutes:D2}",
            Late = setting.LateCount
        });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest req)
    {
        var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Id == req.Id || s.Id == 1);
        if (setting == null)
        {
            setting = new Setting { Id = 1 };
            _context.Settings.Add(setting);
        }

        if (TimeSpan.TryParse(req.StartTime, out var startTime))
        {
            setting.StartTime = startTime;
        }
        if (TimeSpan.TryParse(req.EndTime, out var endTime))
        {
            setting.EndTime = endTime;
        }
        setting.LateCount = req.LateCount;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Settings updated successfully" });
    }
}
