using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using TBalans.Domain.Entities;
using TBalans.Domain.Enums;

namespace TBalans.Infrastructure;

/// <summary>
/// Uygulamanın veritabanı erişim bağlamı. Fluent API ayarlarını ve veri kümelerini içerir.
/// </summary>
public class TBalansDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public DbSet<Assignment> Assignments { get; set; } = null!;
    public DbSet<Schedule> Schedules { get; set; } = null!;
    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<GroupMember> GroupMembers { get; set; } = null!;
    public DbSet<GroupAssignment> GroupAssignments { get; set; } = null!;
    public DbSet<AssignmentComment>    AssignmentComments    { get; set; } = null!;
    public DbSet<AssignmentCompletion> AssignmentCompletions { get; set; } = null!;
    public DbSet<GroupAssignmentSubmission> GroupAssignmentSubmissions { get; set; } = null!;

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

        // AssignmentComment tablosu için konfigürasyonlar
        modelBuilder.Entity<AssignmentComment>(entity =>
        {
            entity.HasOne(ac => ac.GroupAssignment)
                  .WithMany(ga => ga.Comments)
                  .HasForeignKey(ac => ac.GroupAssignmentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ac => ac.User)
                  .WithMany()
                  .HasForeignKey(ac => ac.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Group tablosu için konfigürasyonlar
        modelBuilder.Entity<Group>(entity => 
        {
            entity.HasIndex(g => g.InviteCode)
                  .IsUnique();
        });

        // Assignment tablosu için konfigürasyonlar
        modelBuilder.Entity<Assignment>(entity =>
        {
            // Semester enum verisinin string olarak kaydedilmesi
            entity.Property(a => a.Semester)
                  .HasConversion<string>();
        });

        // Group tablosu için konfigürasyonlar
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Name).IsRequired().HasMaxLength(150);
            entity.Property(g => g.Description).HasMaxLength(500);
            entity.Property(g => g.Theme).HasMaxLength(50);
            entity.Property(g => g.Privacy).HasConversion<string>();

            // Group → CreatedBy (many groups can be created by one user)
            entity.HasOne(g => g.CreatedBy)
                  .WithMany()
                  .HasForeignKey(g => g.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinse grup korunur
        });

        // GroupMember tablosu için konfigürasyonlar
        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(gm => gm.Id);
            entity.Property(gm => gm.Role).HasConversion<string>();

            // Aynı kullanıcı aynı gruba iki kez üye olamaz
            entity.HasIndex(gm => new { gm.GroupId, gm.UserId }).IsUnique();

            // GroupMember → Group (Many-to-One)
            entity.HasOne(gm => gm.Group)
                  .WithMany(g => g.Members)
                  .HasForeignKey(gm => gm.GroupId)
                  .OnDelete(DeleteBehavior.Cascade); // Grup silinince üyelikler de silinir

            // GroupMember → User (Many-to-One)
            entity.HasOne(gm => gm.User)
                  .WithMany()
                  .HasForeignKey(gm => gm.UserId)
                  .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinse üyelikler korunur
        });
        // GroupAssignment tablosu için konfigürasyonlar
        modelBuilder.Entity<GroupAssignment>(entity =>
        {
            entity.HasKey(ga => ga.Id);
            entity.Property(ga => ga.Title).IsRequired().HasMaxLength(200);
            entity.Property(ga => ga.CourseName).IsRequired().HasMaxLength(150);
            entity.Property(ga => ga.Type).HasConversion<string>();

            // GroupAssignment → Group (Many-to-One)
            entity.HasOne(ga => ga.Group)
                  .WithMany()
                  .HasForeignKey(ga => ga.GroupId)
                  .OnDelete(DeleteBehavior.Cascade); // Grup silinince görevler silinir

            // GroupAssignment → CreatedBy (Many-to-One)
            entity.HasOne(ga => ga.CreatedBy)
                  .WithMany()
                  .HasForeignKey(ga => ga.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinse görevler korunur
        });
        // AssignmentCompletion tablosu için konfigürasyon
        modelBuilder.Entity<AssignmentCompletion>(entity =>
        {
            entity.HasKey(ac => ac.Id);

            // Aynı kullanıcı aynı ödevi iki kez tamamlanamaz
            entity.HasIndex(ac => new { ac.GroupAssignmentId, ac.UserId }).IsUnique();

            // AssignmentCompletion -> GroupAssignment (Many-to-One)
            entity.HasOne(ac => ac.GroupAssignment)
                  .WithMany(ga => ga.Completions)
                  .HasForeignKey(ac => ac.GroupAssignmentId)
                  .OnDelete(DeleteBehavior.Cascade);

            // AssignmentCompletion -> User (Many-to-One)
            entity.HasOne(ac => ac.User)
                  .WithMany()
                  .HasForeignKey(ac => ac.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // GroupAssignmentSubmission tablosu için konfigürasyon
        modelBuilder.Entity<GroupAssignmentSubmission>(entity =>
        {
            entity.HasKey(sub => sub.Id);
            entity.Property(sub => sub.MethodDescription).IsRequired();
            entity.Property(sub => sub.ToolsUsed).IsRequired();
            entity.Property(sub => sub.ResultSummary).IsRequired();

            // GroupAssignmentSubmission -> GroupAssignment (Many-to-One)
            entity.HasOne(sub => sub.GroupAssignment)
                  .WithMany(ga => ga.Submissions)
                  .HasForeignKey(sub => sub.GroupAssignmentId)
                  .OnDelete(DeleteBehavior.Cascade); // Görev silinirse değerlendirmesi de silinir.

            // GroupAssignmentSubmission -> User/Student (Many-to-One)
            entity.HasOne(sub => sub.Student)
                  .WithMany()
                  .HasForeignKey(sub => sub.StudentId)
                  .OnDelete(DeleteBehavior.Restrict); // Öğrenci hesabı silinse de teslim kayıtları korunabilir
        });
    }
}
