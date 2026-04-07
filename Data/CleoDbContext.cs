using Microsoft.EntityFrameworkCore;
using cleo.Models;

namespace cleo.Data;

public class CleoDbContext : DbContext
{
    public CleoDbContext(DbContextOptions<CleoDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserAccount> Users { get; set; } = null!;
    public DbSet<AdminMember> Admins { get; set; } = null!;
    public DbSet<ContentArticle> Articles { get; set; } = null!;
    public DbSet<CycleTrack> CycleTracks { get; set; } = null!;
    public DbSet<MoodNote> MoodNotes { get; set; } = null!;
    public DbSet<SymptomLog> SymptomLogs { get; set; } = null!;
    public DbSet<Reminder> Reminders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Seed some data for initial state
        modelBuilder.Entity<AdminMember>().HasData(
            new AdminMember { Id = 1, Name = "Admin", Email = "admin@cleo.app", Password = "password123", IsSuperAdmin = true },
            new AdminMember { Id = 2, Name = "Ava", Email = "ava@cleo.app", Password = "password123", IsSuperAdmin = true },
            new AdminMember { Id = 3, Name = "Hensy", Email = "hensy@cleo.app", Password = "password123", IsSuperAdmin = true }
        );

        modelBuilder.Entity<ContentArticle>().HasData(
            new ContentArticle { Id = 1, Title = "Focus on Iron-Rich Foods", Category = "Nutrition", Content = "Detailed analysis...", Views = 1240 },
            new ContentArticle { Id = 2, Title = "Yoga for Cramp Relief", Category = "Exercise", Content = "Detailed analysis...", Views = 952 },
            new ContentArticle { Id = 3, Title = "Understanding LH Surge", Category = "Science", Content = "Detailed analysis...", Views = 1520 },
            new ContentArticle { Id = 4, Title = "Managing PMS Bloating", Category = "Health", Content = "Detailed analysis...", Views = 840 }
        );
    }
}
