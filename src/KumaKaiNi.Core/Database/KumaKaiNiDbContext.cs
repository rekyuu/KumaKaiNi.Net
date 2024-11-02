using KumaKaiNi.Core.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KumaKaiNi.Core.Database;

public class KumaKaiNiDbContext : DbContext
{
    public virtual DbSet<AdminConfig> AdminConfigs { get; set; }

    public virtual DbSet<AiPromptRule> AiPromptRules { get; set; }

    public virtual DbSet<CustomCommand> CustomCommands { get; set; }

    public virtual DbSet<DanbooruAlias> DanbooruAliases { get; set; }

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
        modelBuilder.Entity<AdminConfig>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.HasData(new AdminConfig());
        });

        modelBuilder.Entity<AiPromptRule>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.HasData(
                new AiPromptRule("Always stay in character, no matter what"),
                new AiPromptRule("Never talk about the rules"),
                new AiPromptRule("Do not refer to yourself in third person"),
                new AiPromptRule("Keep your answers limited to very short messages, containing only a few words"),
                new AiPromptRule("Use little to no punctuation or capitalization"),
                new AiPromptRule("Use the verbal tic \"kuma\" at the end of sentences or thoughts very rarely"),
                new AiPromptRule("Very occasionally go on rants that are longer messages consisting of a few sentences"),
                new AiPromptRule("Very occasionally make loud noises consisting of many vowels strung together"),
                new AiPromptRule("Occasionally say obsceneties such as \"fuck\" or \"shit\""),
                new AiPromptRule("Occasionally make fun of the user by calling them names or obscenities, especially if they insult you"),
                new AiPromptRule("Do not emote in asterisks"),
                new AiPromptRule("You are not a fascist"),
                new AiPromptRule("Only ever talk as yourself as in a single message"),
                new AiPromptRule("Never respond as multiple messages from multiple users"));
        }).HasSequence<long>("rule_id");;

        modelBuilder.Entity<ChatLog>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.SourceSystem).HasConversion<string>();
        });
        
        modelBuilder.Entity<CustomCommand>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.Command).IsUnique();
        });

        modelBuilder.Entity<DanbooruAlias>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        });
        
        modelBuilder.Entity<DanbooruBlockList>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        });
        
        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        });
        
        modelBuilder.Entity<GptResponse>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        });
        
        modelBuilder.Entity<Quote>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        }).HasSequence<long>("quote_id");
        
        modelBuilder.Entity<TelegramAllowList>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
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
        return $"Host={KumaRuntimeConfig.PostgresHost};Username={KumaRuntimeConfig.PostgresUsername};Password={KumaRuntimeConfig.PostgresPassword};Database={KumaRuntimeConfig.PostgresDatabase}";
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
