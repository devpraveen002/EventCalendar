using Calendar.UI.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Calendar.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = AppContext.BaseDirectory,
                Args = args
            });

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            var logger = LoggerFactory.Create(config =>
            {
                config.AddConsole();
            }).CreateLogger<Program>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                logger.LogError("Connection string 'DefaultConnection' not found!");
            }
            else
            {
                logger.LogInformation("Connection string found and is not null");
            }

            builder.Services.AddDbContext<CalendarDbContext>(options =>
                options.UseNpgsql(connectionString));

            var app = builder.Build();

            var webRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            if (!Directory.Exists(webRootPath))
            {
                Directory.CreateDirectory(webRootPath);
                logger.LogInformation($"Created wwwroot directory at: {webRootPath}");
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<CalendarDbContext>();
                    context.Database.EnsureCreated();

                    // Log the actual connection string being used (mask sensitive data)
                    var maskedConnectionString = context.Database.GetConnectionString()
                        ?.Replace(connectionString ?? "", "[MASKED]");
                    logger.LogInformation($"Using connection string: {maskedConnectionString}");

                    context.Database.EnsureCreated();
                    var canConnect = context.Database.CanConnect();
                    logger.LogInformation($"Database connection test: {canConnect}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while initializing the database.");
                }
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
