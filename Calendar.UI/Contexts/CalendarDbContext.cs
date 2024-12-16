using Calendar.UI.Models;
using Microsoft.EntityFrameworkCore;

namespace Calendar.UI.Contexts;

public class CalendarDbContext : DbContext
{
    public CalendarDbContext(DbContextOptions<CalendarDbContext> options) : base(options)
    {

    }

    public DbSet<Event> Events { get; set; }

   
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Date);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.Date)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            // Make UserId optional
            entity.Property(e => e.UserId)
                .IsRequired(false);
        });

    }
}
