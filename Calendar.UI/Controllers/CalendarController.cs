using Calendar.UI.Contexts;
using Calendar.UI.Models;
using Calendar.UI.Views.ViewModels;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Diagnostics;

namespace Calendar.UI.Controllers;

public class CalendarController : Controller
{
    public readonly CalendarDbContext _context;
    private readonly ILogger<CalendarController> _logger;

    public CalendarController(CalendarDbContext context, ILogger<CalendarController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IActionResult Index(DateTime? date)
    {
        var currentDate = date?.Date ?? DateTime.UtcNow.Date;

        // Convert to UTC
        var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        try
        {
            // Get events for the entire month
            var events = _context.Events
                .Where(e => e.Date >= firstDayOfMonth && e.Date <= lastDayOfMonth)
                .OrderBy(e => e.Date)
                .ToList();

            var viewModel = new CalendarViewModel
            {
                CurrentDate = currentDate,
                Events = events
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching calendar events");
            throw;
        }
    }


    // GET: Show add event form
    public IActionResult AddEvent(DateTime date)
    {
        try
        {
            _logger.LogInformation($"Showing AddEvent form for date: {date}");
            var eventModel = new Event { Date = date };
            return View("AddEvent", eventModel);  // Changed from PartialView to View
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing AddEvent form");
            TempData["Error"] = "Unable to show event form. Please try again.";
            return RedirectToAction(nameof(Index), new { date = date });
        }
    }

    // POST: Handle event creation
    [HttpPost]
    public IActionResult AddEvent(Event eventModel)
    {
        _logger.LogInformation($"Received event submission: {eventModel.Title} for date {eventModel.Date}");

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Model state is invalid");
            return View(eventModel);
        }

        try
        {
            // Ensure UTC date
            eventModel.Date = DateTime.SpecifyKind(eventModel.Date.Date, DateTimeKind.Utc);

            _context.Events.Add(eventModel);
            var result = _context.SaveChanges();

            _logger.LogInformation($"Saved event with result: {result}");

            TempData["Success"] = "Event added successfully!";
            return RedirectToAction(nameof(Index), new { date = eventModel.Date.ToString("yyyy-MM-dd") });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving event");
            ModelState.AddModelError("", "Failed to save event. Please try again.");
            return View(eventModel);
        }
    }


    public IActionResult ExportToExcel(DateTime date)
    {
        try
        {
            // Get first and last day of the selected month
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Get events for the selected month
            var events = _context.Events
                .Where(e => e.Date >= firstDayOfMonth && e.Date <= lastDayOfMonth)
                .OrderBy(e => e.Date)
                .ToList();

            // Configure EPPlus to use non-commercial license
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Events");

                // Set headers
                worksheet.Cells["A1"].Value = "Date";
                worksheet.Cells["B1"].Value = "Title";
                worksheet.Cells["C1"].Value = "Description";

                // Style header row
                var headerRange = worksheet.Cells["A1:C1"];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // Add data
                int row = 2;
                foreach (var evt in events)
                {
                    worksheet.Cells[row, 1].Value = evt.Date.ToLocalTime().ToString("MM/dd/yyyy");
                    worksheet.Cells[row, 2].Value = evt.Title;
                    worksheet.Cells[row, 3].Value = evt.Description;
                    row++;
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Set content type and filename
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = $"Calendar_Events_{date:MMMM_yyyy}.xlsx";

                // Return the Excel file
                return File(package.GetAsByteArray(), contentType, fileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to Excel");
            TempData["Error"] = "Failed to export to Excel. Please try again.";
            return RedirectToAction(nameof(Index), new { date });
        }
    }

    public IActionResult ExportToCsv(DateTime date)
    {
        try
        {
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);

            var events = _context.Events
                .Where(e => e.Date >= firstDayOfMonth && e.Date <= lastDayOfMonth)
                .OrderBy(e => e.Date)
                .ToList();

            // Add UTF-8 BOM
            byte[] bom = new byte[] { 0xEF, 0xBB, 0xBF };
            memoryStream.Write(bom, 0, bom.Length);

            // Write headers
            writer.WriteLine("Date,Title,Description");

            // Write data with explicit date formatting
            foreach (var evt in events)
            {
                // Format date as string with quotes to force Excel to treat it as text
                var dateStr = $"\"{evt.Date.ToString("yyyy-MM-dd")}\"";
                var title = EscapeCsvField(evt.Title);
                var description = EscapeCsvField(evt.Description ?? "");

                writer.WriteLine($"{dateStr},{title},{description}");
            }

            writer.Flush();
            memoryStream.Position = 0;

            return File(
                memoryStream.ToArray(),
                "application/vnd.ms-excel; charset=utf-8",  // Changed content type
                $"Calendar_Events_{date:MMMM_yyyy}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to CSV");
            TempData["Error"] = "Failed to export to CSV. Please try again.";
            return RedirectToAction(nameof(Index), new { date });
        }
    }

    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field)) return "\"\"";
        // Escape quotes and wrap field in quotes
        return $"\"{field.Replace("\"", "\"\"")}\"";
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

}
