using KumaKaiNi.Core.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KumaKaiNi.Core.Database;

public class KumaKaiNiDbContext : DbContext
{
    public virtual DbSet<CustomCommand> CustomCommands { get; set; }

    public virtual DbSet<DanbooruBlockList> DanbooruBlockList { get; set; }

    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

    public virtual DbSet<GptResponse> GptResponses { get; set; }

    public virtual DbSet<ChatLog> ChatLogs { get; set; }

    public virtual DbSet<Quote> Quotes { get; set; }

    public virtual DbSet<TelegramAllowList> TelegramAllowList { get; set; }
    
    public KumaKaiNiDbContext() {}

    public KumaKaiNiDbContext(DbContextOptions<KumaKaiNiDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(GetPostgresConnectionString());
        
        // https://www.npgsql.org/doc/types/datetime.html
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatLog>(entity =>
        {
            entity.Property(e => e.SourceSystem).HasConversion<string>();
        });
        
        modelBuilder.Entity<CustomCommand>(entity =>
        {
            entity.HasIndex(e => e.Command).IsUnique();
        });
    }

    public override int SaveChanges()
    {
        UpdateBaseDateTimeFields();   
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateBaseDateTimeFields();  
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        UpdateBaseDateTimeFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new())
    {
        UpdateBaseDateTimeFields();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private static string GetPostgresConnectionString()
    {
        return $"Host={KumaConfig.PostgresHost};Username={KumaConfig.PostgresUsername};Password={KumaConfig.PostgresPassword};Database={KumaConfig.PostgresDatabase}";
    }

    private void UpdateBaseDateTimeFields()
    {
        IEnumerable<EntityEntry> entries = ChangeTracker
            .Entries()
            .Where(e => e is
            {
                Entity: BaseDbEntity, 
                State: EntityState.Added or EntityState.Modified
            });

        foreach (EntityEntry entry in entries)
        {
            ((BaseDbEntity)entry.Entity).LastModified = DateTime.UtcNow;
            if (entry.State == EntityState.Added)
            {
                ((BaseDbEntity)entry.Entity).InsertedAt = DateTime.UtcNow;
            }
        }
    }
}
