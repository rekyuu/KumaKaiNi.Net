using System.Reflection;
using KumaKaiNi.Core.Database;
using KumaKaiNi.Core.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog.Core;
using Serilog.Events;

namespace KumaKaiNi.Core;

public static class KumaRuntimeConfig
{
    /// <summary>
    /// Name of the application, set by the project's Product tag.
    /// </summary>
    public static string ApplicationName { get; private set; } = "KumaKaiNi";

    /// <summary>
    /// Version of the application, set by the project's Version tag using the BUILD_VERSION environment variable.
    /// </summary>
    public static string ApplicationVersion { get; private set; } = "0.0.0-alpha";
    
    /// <summary>
    /// The git hash of the build.
    /// </summary>
    public static string? BuildCommit { get; private set; }
    
    /// <summary>
    /// The domain and port of the Redis instance.
    /// </summary>
    public static string RedisHost { get; private set; }
    
    /// <summary>
    /// The password for the Redis instance.
    /// </summary>
    public static string RedisPassword { get; private set; }
    
    /// <summary>
    /// The username for the Danbooru API.
    /// </summary>
    public static string? DanbooruUser { get; private set; }
        
    /// <summary>
    /// The API key for the Danbooru API.
    /// </summary>
    public static string? DanbooruApiKey { get; private set; }
    
    /// <summary>
    /// The API key for OpenAI.
    /// </summary>
    public static string? OpenAiApiKey { get; private set; }
        
    /// <summary>
    /// The domain and port for PostgreSQL.
    /// </summary>
    public static string PostgresHost { get; private set; }
        
    /// <summary>
    /// The username for PostgreSQL.
    /// </summary>
    public static string PostgresUsername { get; private set; }
        
    /// <summary>
    /// The password for PostgreSQL.
    /// </summary>
    public static string PostgresPassword { get; private set; }
        
    /// <summary>
    /// The PostgreSQL database to connect to.
    /// </summary>
    public static string PostgresDatabase { get; private set; }
    
    /// <summary>
    /// The user ID of the administrator.
    /// </summary>
    public static long? TelegramAdminId { get; private set; }

    /// <summary>
    /// The log level of the application. Set by the LOG_LEVEL environment variable.
    /// </summary>
    public static string LogLevel { get; private set; }

    static KumaRuntimeConfig()
    {
        Assembly? assembly = Assembly.GetEntryAssembly();

        AssemblyProductAttribute? product = assembly?.GetCustomAttribute<AssemblyProductAttribute>();
        AssemblyInformationalVersionAttribute? version = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        if (!string.IsNullOrEmpty(product?.Product)) ApplicationName = product.Product;
        if (!string.IsNullOrEmpty(version?.InformationalVersion)) ApplicationVersion = version.InformationalVersion;

        // Any null fallbacks should only be used for local testing
        
        BuildCommit = Environment.GetEnvironmentVariable("COMMIT_SHA") ?? "unknown";
        
        RedisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost:6379";
        RedisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD") ?? "redis";
        
        DanbooruUser = Environment.GetEnvironmentVariable("DANBOORU_USER");
        DanbooruApiKey = Environment.GetEnvironmentVariable("DANBOORU_API_KEY");
        
        OpenAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            
        PostgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost:5432";
        PostgresUsername = Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? "postgres";
        PostgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";
        PostgresDatabase = Environment.GetEnvironmentVariable("POSTGRES_DATABASE") ?? "kumakaini";

        string? telegramAdminId = Environment.GetEnvironmentVariable("TELEGRAM_ADMIN_ID");
        bool telegramAdminIdParsed = long.TryParse(telegramAdminId, out long telegramAdminIdResult);
        if (telegramAdminIdParsed) TelegramAdminId = telegramAdminIdResult;
        
        LogLevel = Environment.GetEnvironmentVariable("LOG_LEVEL") ?? "INFO";
    }

    public static LoggingLevelSwitch GetLogLevel()
    {
        LoggingLevelSwitch loggingLevelSwitch = new();

        switch (LogLevel)
        {
            case "VERBOSE":
                loggingLevelSwitch.MinimumLevel = LogEventLevel.Verbose;
                break;
            case "DEBUG":
                loggingLevelSwitch.MinimumLevel = LogEventLevel.Debug;
                break;
            case "WARN":
                loggingLevelSwitch.MinimumLevel = LogEventLevel.Warning;
                break;
            case "ERROR":
                loggingLevelSwitch.MinimumLevel = LogEventLevel.Error;
                break;
            case "FATAL":
                loggingLevelSwitch.MinimumLevel = LogEventLevel.Fatal;
                break;
            // ReSharper disable once RedundantCaseLabel
            case "INFO":
            default:
                loggingLevelSwitch.MinimumLevel = LogEventLevel.Information;
                break;
        }

        return loggingLevelSwitch;
    }

    public static async Task<AdminConfig> GetConfigFromDatabaseAsync()
    {
        await using KumaKaiNiDbContext db = new();

        AdminConfig? config = await db.AdminConfigs.FirstOrDefaultAsync();
        if (config != null) return config;

        config = new AdminConfig();
        await db.AdminConfigs.AddAsync(config);
        await db.SaveChangesAsync();

        return config;
    }
}