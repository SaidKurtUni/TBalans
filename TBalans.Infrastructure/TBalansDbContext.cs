using Microsoft.EntityFrameworkCore;
using TBalans.Domain.Entities;
using TBalans.Domain.Enums;

namespace TBalans.Infrastructure;

/// <summary>
/// Uygulamanın veritabanı erişim bağlamı. Fluent API ayarlarını ve veri kümelerini içerir.
/// </summary>
public class TBalansDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Schedule> Schedules { get; set; }

    // Parametresiz kurucu - Migration komutlarında gerekebilir
    public TBalansDbContext() { }

    public TBalansDbContext(DbContextOptions<TBalansDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=tbalans.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User tablosu için konfigürasyonlar
        modelBuilder.Entity<User>(entity =>
        {
            // İstenen özelliklerin veritabanında varsayılan değerleri (Fluent API Default Value)
            entity.Property(u => u.University)
                  .HasDefaultValue("Bandırma Onyedi Eylül Üniversitesi");

            entity.Property(u => u.Department)
                  .HasDefaultValue("Yazılım Mühendisliği");

            // Semester enum verisinin int yerine string (örn: "Fall", "Spring") şeklinde veritabanına kaydedilmesi
            entity.Property(u => u.Semester)
                  .HasConversion<string>();
        });

        // Assignment tablosu için konfigürasyonlar
        modelBuilder.Entity<Assignment>(entity =>
        {
            // Semester enum verisinin string olarak kaydedilmesi
            entity.Property(a => a.Semester)
                  .HasConversion<string>();
        });
    }
}
