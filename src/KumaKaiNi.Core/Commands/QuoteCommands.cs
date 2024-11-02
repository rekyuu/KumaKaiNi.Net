using KumaKaiNi.Core.Attributes;
using KumaKaiNi.Core.Database;
using KumaKaiNi.Core.Database.Entities;
using KumaKaiNi.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KumaKaiNi.Core.Commands;

public static class QuoteCommands
{
    private const string? ErrorResponse = "Usage: !quote [add <quote> | del <quote_id>]";

    [Command("quote")]
    public static async Task<KumaResponse?> QuoteAsync(KumaRequest kumaRequest)
    {
        switch (kumaRequest.CommandArgs.Length)
        {
            // Get a random Quote
            case 0:
            {
                Quote? quote = await GetQuote();
                return quote != null ? new KumaResponse($"[{quote.QuoteId}] {quote.Text}") : null;
            }
            // Get a specific quote by ID
            case 1:
            {
                bool parsed = long.TryParse(kumaRequest.CommandArgs[0], out long quoteId);
                if (!parsed) return new KumaResponse(ErrorResponse);

                Quote? quote = await GetQuote(quoteId);
                return quote != null ? new KumaResponse($"[{quote.QuoteId}] {quote.Text}") : null;
            }
            case > 1:
            {
                await using KumaKaiNiDbContext db = new();

                string? responseText;
                switch (kumaRequest.CommandArgs[0])
                {
                    // Add a new quote
                    case "add":
                    case "new":
                        if (kumaRequest.UserAuthority == UserAuthority.User) return null;

                        string quoteText = string.Join(" ", kumaRequest.CommandArgs[1..]);
                        Quote newQuote = new(quoteText);
                        await db.Quotes.AddAsync(newQuote);

                        responseText = "Quote added.";
                        break;
                    // Delete an existing quote by ID
                    case "del":
                    case "delete":
                    case "rem":
                    case "remove":
                        if (kumaRequest.UserAuthority == UserAuthority.User) return null;

                        bool parsed = long.TryParse(kumaRequest.CommandArgs[1], out long quoteId);
                        if (!parsed) return new KumaResponse(ErrorResponse);

                        Quote? quote = await GetQuote(quoteId);
                        if (quote != null) db.Remove(quote);
                    
                        responseText = "Quote removed.";
                        break;
                    default:
                        return new KumaResponse(ErrorResponse);
                }

                await db.SaveChangesAsync();
                return new KumaResponse(responseText);
            }
            default:
            {
                return new KumaResponse(ErrorResponse);
            }
        }
    }

    private static async Task<Quote?> GetQuote(long? id = null)
    {
        await using KumaKaiNiDbContext db = new();

        Quote? quote;
        if (id == null)
        {
            quote = await db.Quotes
                .OrderBy(x => Guid.NewGuid())
                .FirstOrDefaultAsync();
        }
        else
        {
            quote = await db.Quotes
                .FirstOrDefaultAsync(x => x.QuoteId == id);
        }

        return quote;
    }
}