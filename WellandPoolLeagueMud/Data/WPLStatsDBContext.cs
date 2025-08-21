using Microsoft.EntityFrameworkCore;
using WellandPoolLeagueMud.Data.Models;

namespace WellandPoolLeagueMud.Data;

public partial class WPLStatsDBContext : DbContext
{
    public WPLStatsDBContext() { }
    
    public WPLStatsDBContext(DbContextOptions<WPLStatsDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<PlayerDatum> PlayerData { get; set; }

    public virtual DbSet<PlayersView> PlayersViews { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<TeamDetail> TeamDetails { get; set; }

    public virtual DbSet<Week> Weeks { get; set; }

    public virtual DbSet<WeeksView> WeeksViews { get; set; }

    public virtual DbSet<Changelog> Changelog { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("wiley");

        modelBuilder.Entity<Player>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<PlayerDatum>(entity =>
        {
            entity.Property(e => e.PlayerId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<PlayersView>(entity =>
        {
            entity.ToView("PlayersView");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Tmp_Schedule");
        });

        modelBuilder.Entity<TeamDetail>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Week>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Week");
        });

        modelBuilder.Entity<WeeksView>(entity =>
        {
            entity.ToView("WeeksView");
        });

        modelBuilder.Entity<Changelog>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_Changelog");
            entity.Property(e => e.ID).ValueGeneratedOnAdd();
        });
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
