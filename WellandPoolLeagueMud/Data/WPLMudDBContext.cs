// Data/WPLMudDBContext.cs
using Microsoft.EntityFrameworkCore;
using WellandPoolLeagueMud.Data.Models;

namespace WellandPoolLeagueMud.Data
{
    public class WPLMudDBContext : DbContext
    {
        public WPLMudDBContext(DbContextOptions<WPLMudDBContext> options) : base(options)
        {
        }

        public DbSet<WPL_Player> Players { get; set; }
        public DbSet<WPL_Team> Teams { get; set; }
        public DbSet<WPL_Schedule> Schedules { get; set; }
        public DbSet<WPL_WeeklyWinner> WeeklyWinners { get; set; }
        public DbSet<WPL_PlayerGame> PlayerGames { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Set the default schema for the entire database
            modelBuilder.HasDefaultSchema("wiley");

            // All tables will now be created in the 'wiley' schema by default
            modelBuilder.Entity<WPL_Player>().ToTable("WPL_Players");
            modelBuilder.Entity<WPL_Team>().ToTable("WPL_Teams");
            modelBuilder.Entity<WPL_Schedule>().ToTable("WPL_Schedules");
            modelBuilder.Entity<WPL_WeeklyWinner>().ToTable("WPL_WeeklyWinners");
            modelBuilder.Entity<WPL_PlayerGame>().ToTable("WPL_PlayerGames");

            // Schedule-Team relationship
            modelBuilder.Entity<WPL_Schedule>()
                .HasOne(s => s.HomeTeam)
                .WithMany()
                .HasForeignKey(s => s.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WPL_Schedule>()
                .HasOne(s => s.AwayTeam)
                .WithMany()
                .HasForeignKey(s => s.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // WeeklyWinner-Team relationship
            modelBuilder.Entity<WPL_WeeklyWinner>()
                .HasOne(ww => ww.WinningTeam)
                .WithMany()
                .HasForeignKey(ww => ww.WinningTeamId);

            // PlayerGame-Player relationship
            modelBuilder.Entity<WPL_PlayerGame>()
                .HasOne(pg => pg.Player)
                .WithMany(p => p.PlayerGames)
                .HasForeignKey(pg => pg.PlayerId);

            // PlayerGame-Schedule relationship
            modelBuilder.Entity<WPL_PlayerGame>()
                .HasOne(pg => pg.Schedule)
                .WithMany()
                .HasForeignKey(pg => pg.ScheduleId);
        }
    }
}