using Microsoft.EntityFrameworkCore;
using wt_lab7_7postgres_efcore_findrik.Models;

namespace wt_lab7_7postgres_efcore_findrik.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ItTask> Tasks => Set<ItTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ItTask>().HasData(
            new ItTask 
            { 
                Id = 1, 
                Title = "Разработка API", 
                ProjectName = "CRM System", 
                Priority = "High", 
                EstimatedHours = 40, 
                IsCompleted = false, 
                CreatedAt = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc) 
            },
            new ItTask 
            { 
                Id = 2, 
                Title = "Дизайн интерфейса", 
                ProjectName = "CRM System", 
                Priority = "Medium", 
                EstimatedHours = 20, 
                IsCompleted = true, 
                CreatedAt = DateTime.SpecifyKind(new DateTime(2026, 1, 2), DateTimeKind.Utc) 
            },
            new ItTask 
            { 
                Id = 3, 
                Title = "Тестирование", 
                ProjectName = "Mobile Banking", 
                Priority = "High", 
                EstimatedHours = 30, 
                IsCompleted = false, 
                CreatedAt = DateTime.SpecifyKind(new DateTime(2026, 1, 3), DateTimeKind.Utc) 
            }
        );
    }
}