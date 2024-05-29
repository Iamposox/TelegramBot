using Microsoft.EntityFrameworkCore;

namespace TelegramBusinessTripBot.WebApp.Entities;

public class TgContext : DbContext
{
    public TgContext(DbContextOptions<TgContext> options) : base(options)
    {
    }
    public DbSet<Users> Users { get; set; }
    public DbSet<ChatOrChannel> ChatOrChannels{ get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Users>()
                    .HasMany(left => left.ChatOrChannel)
                    .WithMany(right => right.Users)
                    .RightNavigation?.ForeignKey?.SetConstraintName("FK_UsersChatOrChannel_Users");
        modelBuilder.Entity<Users>()
                    .HasMany(left => left.ChatOrChannel)
                    .WithMany(right => right.Users)
                    .LeftNavigation?.ForeignKey?.SetConstraintName("FK_UsersChatOrChannel_ChatOrChannel");
        modelBuilder.Entity<Users>()
                    .HasMany(left => left.ChatOrChannel)
                    .WithMany(right => right.Users)
                    .UsingEntity(join => join.ToTable("UsersChatOrChannel"));
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
