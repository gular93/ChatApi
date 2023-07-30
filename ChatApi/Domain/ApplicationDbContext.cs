using ChatApi.Domain;
using ChatApi.Domain.Enum;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<Agent> Agents { get; set; }
    public DbSet<Team> Teams { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("ChatDb");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agent>()
            .HasOne(a => a.Team)
            .WithMany(t => t.Agents)
            .HasForeignKey(a => a.TeamId);

        // Seed data
        modelBuilder.Entity<Team>().HasData(new Team
        {
            Id = 1,
            Name = "Team 1"
        },
        new Team
        {
            Id = 2,
            Name = "Team 2"
        });

        modelBuilder.Entity<Agent>().HasData(
            new Agent
            {
                Id = 1,
                Name = "Agent 1", 
                CurrentChatCount = 2,             
                TeamId = 1,
                Seniority = SeniorityLevel.Junior,
                Shift = Shift.Evening
            },
            new Agent
            {
                Id = 2,
                Name = "Agent 2",
                CurrentChatCount = 3,
                TeamId = 1,
                Seniority = SeniorityLevel.TeamLead,
                Shift = Shift.Morning
            });

        modelBuilder.Entity<ChatSession>().HasData(
            new ChatSession
            {
                Id = 1,
                StartTime = DateTime.UtcNow,
                LastPollTime = DateTime.UtcNow,
                IsActive = true,
                AssignedAgentId = 1
            });
    }
}
