using ClosedXML.Excel;
using ClassAssistant.Shared.DTOs;

namespace ClassAssistant.Backend.Services;

public interface IExportService
{
    byte[] ExportAttendanceMatrixToExcel(List<StudentProfileDto> students, List<AttendanceLogDto> attendances, string courseCode, string dateStr);
}

public class ExportService : IExportService
{
    public byte[] ExportAttendanceMatrixToExcel(List<StudentProfileDto> students, List<AttendanceLogDto> attendances, string courseCode, string dateStr)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Attendance Summary");

        worksheet.Cell(1, 1).Value = "ClassAssistant AI - Attendance Matrix Report";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
        worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#059669");

        worksheet.Cell(2, 1).Value = $"Course: {courseCode} | Date: {dateStr}";
        worksheet.Cell(2, 1).Style.Font.Italic = true;

        worksheet.Cell(4, 1).Value = "Student ID";
        worksheet.Cell(4, 2).Value = "Student Name";
        worksheet.Cell(4, 3).Value = "Course Code";
        worksheet.Cell(4, 4).Value = "Attendance Date";
        worksheet.Cell(4, 5).Value = "Check In Time";
        worksheet.Cell(4, 6).Value = "Check Out Time";
        worksheet.Cell(4, 7).Value = "Total Minutes";
        worksheet.Cell(4, 8).Value = "Status";

        var headerRange = worksheet.Range("A4:H4");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#10B981");
        headerRange.Style.Font.FontColor = XLColor.White;

        int row = 5;
        foreach (var att in attendances)
        {
            worksheet.Cell(row, 1).Value = att.Id;
            worksheet.Cell(row, 2).Value = att.Course;
            worksheet.Cell(row, 3).Value = courseCode;
            worksheet.Cell(row, 4).Value = att.Date;
            worksheet.Cell(row, 5).Value = att.CheckIn;
            worksheet.Cell(row, 6).Value = att.CheckOut;
            worksheet.Cell(row, 7).Value = att.TotalMinutes;
            worksheet.Cell(row, 8).Value = att.Status;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
