using Calendar.UI.Models;

namespace Calendar.UI.Views.ViewModels;

public class CalendarViewModel
{
    public DateTime CurrentDate { get; set; }
    public List<Event> Events { get; set; } = new List<Event>();
}
