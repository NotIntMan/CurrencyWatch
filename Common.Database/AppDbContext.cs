using Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Common.Database;

public class AppDbContext : DbContext
{
    public const int DefaultMaxLength = 256;

    public DbSet<User> Users => Set<User>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<UserFavoriteCurrency> UserFavorites => Set<UserFavoriteCurrency>();
    public DbSet<TokenBlacklistEntry> TokenBlacklist => Set<TokenBlacklistEntry>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DefaultMaxLength).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.PasswordHash).HasMaxLength(DefaultMaxLength).IsRequired();
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.ToTable("currencies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DefaultMaxLength).IsRequired();
            entity.Property(e => e.CharCode).HasMaxLength(10).IsRequired();
            entity.HasIndex(e => e.CharCode).IsUnique();
            entity.Property(e => e.Rate).HasPrecision(18, 6);
        });

        modelBuilder.Entity<UserFavoriteCurrency>(entity =>
        {
            entity.ToTable("user_favorites");
            entity.HasKey(e => new { e.UserId, e.CurrencyId });

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Currency>()
                .WithMany()
                .HasForeignKey(e => e.CurrencyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TokenBlacklistEntry>(entity =>
        {
            entity.ToTable("token_blacklist");
            entity.HasKey(e => e.Jti);
            entity.Property(e => e.Jti).HasMaxLength(DefaultMaxLength);
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.HasIndex(e => e.ExpiresAt);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).HasMaxLength(DefaultMaxLength).IsRequired();
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.ExpiresAt).IsRequired();

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
