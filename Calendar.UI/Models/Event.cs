using System.ComponentModel.DataAnnotations;

namespace Calendar.UI.Models;

public class Event
{
    public int Id { get; set; }
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }
    [MaxLength(1000)]
    public string Description { get; set; }

    private DateTime _date;
    [Required]
    public DateTime Date
    {
        get => _date;
        set => _date = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
    public string? UserId { get; set; }
}
