using KumaKaiNi.Core.Attributes;
using KumaKaiNi.Core.Models;
using KumaKaiNi.Core.Utility;

namespace KumaKaiNi.Core.Commands;

public static class GeneralCommands
{
    private static readonly string[] PingResponses = ["Kuma?", "Kuma~!", "Kuma...", "Kuma!!", "Kuma.", "Kuma...?"];
    private static readonly string[] HelloResponses = ["sup", "yo", "ay", "hi", "wassup"];
    private static readonly string[] Choices = ["np", "don't mention it", "anytime", "sure thing", "ye whateva"];

    [Command("about")]
    public static KumaResponse About()
    {
        return new KumaResponse($"KumaKaiNi `{KumaConfig.ApplicationVersion}`\nCommit `{KumaConfig.BuildCommit}`\nhttps://github.com/rekyuu/KumaKaiNi.Net");
    }

    [Command(["kuma", "ping"])]
    public static KumaResponse Ping()
    {
        string reply = Rng.PickRandom(PingResponses);
        return new KumaResponse(reply);
    }

    [Command("say")]
    public static KumaResponse? Say(KumaRequest kumaRequest)
    {
        return kumaRequest.CommandArgs.Length > 0 ? new KumaResponse(string.Join(" ", kumaRequest.CommandArgs)) : null;
    }

    [Phrase(["hi", "hello", "hey", "sup"])]
    public static KumaResponse? Hello()
    {
        return Rng.OneTo(25) ? new KumaResponse(Rng.PickRandom(HelloResponses)) : null;
    }

    [Phrase(["ty kuma", "thanks kuma", "thank you kuma"])]
    public static KumaResponse Thanks()
    {
        return new KumaResponse(Rng.PickRandom(Choices));
    }

    [Phrase("same")]
    public static KumaResponse? Same()
    {
        return Rng.OneTo(25) ? new KumaResponse("same") : null;
    }

    [Command("projection")]
    public static KumaResponse Projection()
    {
        return new KumaResponse("Psychological projection is a theory in psychology in which humans defend themselves against their own unpleasant impulses by denying their existence while attributing them to others. For example, a person who is rude may constantly accuse other people of being rude. It can take the form of blame shifting.");
    }

    [Command("moon")]
    public static KumaResponse MoonPhase()
    {
        int phase = Moon.GetMoonPhase(DateTime.UtcNow);

        return phase switch
        {
            29 => new KumaResponse("New Moon"),
            >= 23 => new KumaResponse("Waning Crescent"),
            22 => new KumaResponse("Last Quarter"),
            >= 16 => new KumaResponse("Waning Gibbous"),
            15 => new KumaResponse("Full Moon"),
            >= 9 => new KumaResponse("Waxing Gibbous"),
            8 => new KumaResponse("First Quarter"),
            >= 2 => new KumaResponse("Waxing Crescent"),
            1 => new KumaResponse("New Moon"),
            _ => new KumaResponse("N̸̛̹̩͈͖̭̤͚̠̘̝̮͓͈̻̾̈̋̇͊̀̔̚͠͝e̷̡̧̼̩̰̼̞̖͙̮̙̰̳͑̇̑̽͆͆̇̍͘͠w̷̞̦̪̑̎̆̒ ̸̛̩͇̹̯̠̊͆̊Ḿ̵̲͕͔̼̘͙͍͇ͅơ̷̧̙͈͍̻̯̬͔̈́̐̂͌̏̚͘͜o̴̢͕̫̱̪̬̤̱̳͈̩̤̐̈͋̎̅́̿̏̊̕n̶̰̼̯̼͇͕̥̭̞̖͖̾̎́̄͆͂͋̽͌ͅ")
        };
    }
}