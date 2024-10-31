using KumaKaiNi.Core.Attributes;
using KumaKaiNi.Core.Database;
using KumaKaiNi.Core.Database.Entities;
using KumaKaiNi.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KumaKaiNi.Core.Commands;

public static class AdminConfigCommands
{
    [Command("gptmodel", UserAuthority.Administrator)]
    public static async Task<KumaResponse?> UpdateGptModel(KumaRequest kumaRequest)
    {
        if (kumaRequest.CommandArgs.Length > 0) return null;

        string model = kumaRequest.CommandArgs.First();

        AdminConfig config = await GetAdminConfig();
        config.OpenAiModel = model;

        await using KumaKaiNiDbContext db = new();

        // TODO: Do something here probably

        return new KumaResponse($"OpenAI GPT model changed to \"{model}\".");
    }

    private static async Task<AdminConfig> GetAdminConfig()
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