using KumaKaiNi.Core.Attributes;
using KumaKaiNi.Core.Database;
using KumaKaiNi.Core.Database.Entities;
using KumaKaiNi.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KumaKaiNi.Core.Commands;

public static class TelegramAllowlistCommands
{
    [Command(["approve", "allow"], UserAuthority.Administrator)]
    public static async Task<KumaResponse?> AddToAllowListAsync(KumaRequest request)
    {
        if (request.SourceSystem != SourceSystem.Telegram) return null;
        if (request.ChannelId == null) return null;
        
        await using KumaKaiNiDbContext db = new();

        TelegramAllowList? allowList = await db.TelegramAllowList
            .FirstOrDefaultAsync(x => x.ChannelId == request.ChannelId);

        if (allowList?.Approved == true) return new KumaResponse("Chat is already approved.");

        if (allowList == null)
        {
            allowList = new TelegramAllowList(request.ChannelId);
            await db.TelegramAllowList.AddAsync(allowList);
        }

        allowList.Approved = true;
        allowList.Warnings = 0;

        await db.SaveChangesAsync();

        return new KumaResponse("Added to allowlist.");
    }
}