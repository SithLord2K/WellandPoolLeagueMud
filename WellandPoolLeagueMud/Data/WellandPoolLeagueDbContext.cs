using Microsoft.EntityFrameworkCore;
using WellandPoolLeagueMud.Data.Models;

namespace WellandPoolLeagueMud.Data
{
    public class WellandPoolLeagueDbContext : DbContext
    {
        public WellandPoolLeagueDbContext(DbContextOptions<WellandPoolLeagueDbContext> options) : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<PlayerGame> PlayerGames { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Player>().ToTable("WPLMud_Players");
            modelBuilder.Entity<Team>().ToTable("WPLMud_Teams");
            modelBuilder.Entity<PlayerGame>().ToTable("WPLMud_PlayerGames");
            modelBuilder.Entity<Schedule>().ToTable("WPLMud_Schedules");
            modelBuilder.Entity<UserProfile>().ToTable("WPLMud_UserProfiles");

            modelBuilder.Entity<Team>()
                .HasMany(t => t.Players)
                .WithOne(p => p.Team)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Captain)
                .WithMany(p => p.CaptainedTeams)
                .HasForeignKey(t => t.CaptainPlayerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PlayerGame>()
                .HasOne(pg => pg.Player)
                .WithMany(p => p.PlayerGames)
                .HasForeignKey(pg => pg.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlayerGame>()
                .HasOne(pg => pg.Team)
                .WithMany(t => t.PlayerGames)
                .HasForeignKey(pg => pg.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.HomeTeam)
                .WithMany(t => t.HomeGames)
                .HasForeignKey(s => s.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.AwayTeam)
                .WithMany(t => t.AwayGames)
                .HasForeignKey(s => s.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.WinningTeam)
                .WithMany(t => t.WonGames)
                .HasForeignKey(s => s.WinningTeamId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Player>()
                .HasIndex(p => new { p.FirstName, p.LastName });

            modelBuilder.Entity<Team>()
                .HasIndex(t => t.TeamName)
                .IsUnique();

            modelBuilder.Entity<PlayerGame>()
                .HasIndex(pg => new { pg.PlayerId, pg.WeekNumber, pg.TeamId });

            modelBuilder.Entity<Schedule>()
                .HasIndex(s => new { s.WeekNumber, s.GameDate });

            modelBuilder.Entity<UserProfile>()
                .HasIndex(p => p.Auth0UserId)
                .IsUnique();
        }
    }
}