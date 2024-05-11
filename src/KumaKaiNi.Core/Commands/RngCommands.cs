using KumaKaiNi.Core.Attributes;
using KumaKaiNi.Core.Models;
using KumaKaiNi.Core.Utility;

namespace KumaKaiNi.Core.Commands;

public static class RngCommands
{
    private static readonly string[] CoinChoices = ["Heads.", "Tails."];
    private static readonly string[] PredictionChoices =
    [
        "It is certain.",
        "It is decidedly so.",
        "Without a doubt.",
        "Yes, definitely.",
        "You may rely on it.",
        "As I see it, yes.",
        "Most likely.",
        "Outlook good.",
        "Yes.",
        "Signs point to yes.",
        "Reply hazy, try again.",
        "Ask again later.",
        "Better not tell you now.",
        "Cannot predict now.",
        "Concentrate and ask again.",
        "Don't count on it.",
        "My reply is no.",
        "My sources say no.",
        "Outlook not so good.",
        "Very doubtful."
    ];

    [Command(["coin", "flip"])]
    public static KumaResponse CoinFlip()
    {
        string result = Rng.PickRandom(CoinChoices);
        return new KumaResponse(result);
    }

    [Command(["pick", "choose"])]
    public static KumaResponse PickOne(KumaRequest kumaRequest)
    {
        if (kumaRequest.CommandArgs.Length < 2) return new KumaResponse("Usage: !pick option1, option2, [option3, ...]");

        string[] commaSplitArgs = string.Join(" ", kumaRequest.CommandArgs).Split(", ");

        string result = Rng.PickRandom(commaSplitArgs);
        return new KumaResponse(result);
    }

    [Command(["roll", "dice"])]
    public static KumaResponse RollDice(KumaRequest kumaRequest)
    {
        Random rng = new();

        // User supplied an argument (ie, !roll 4 or roll 4d6)
        if (kumaRequest.CommandArgs.Length == 0)
        {
            // Roll a d6.
            int result = rng.Next(1, 7);
            return new KumaResponse(result.ToString());
        }
        
        string[] args = kumaRequest.CommandArgs[0].Split('d');
        int diceAmount = 0;
        int diceSides = 0;

        try
        {
            switch (args.Length)
            {
                // Parse the xdy format.
                case >= 2 when args[0] != "" && args[1] != "":
                    diceAmount = Convert.ToInt32(args[0]);
                    diceSides = Convert.ToInt32(args[1]);
                    break;
                // Parse the number only format.
                case 1 when args[0] != "":
                    diceAmount = 1;
                    diceSides = Convert.ToInt32(args[0]);
                    break;
            }
        }
        catch
        {
            // Assume the arguments given were not proper and return a help message.
            return new KumaResponse("Usage: !roll\n       !roll {dice sides}\n       !roll {dice amount}d{dice sides}\n");
        }

        if (diceAmount <= 0 || diceSides <= 2)
            return new KumaResponse("The dice amount must be 1 or more, and the dice sides must be 3 or more.");
        // Get the results and the sum of the rolls.
        int total = 0;
        List<int> results = [];
        for (int i = 0; i < diceAmount; i++)
        {
            int result = rng.Next(1, diceSides + 1);

            total += result;
            results.Add(result);
        }

        if (diceAmount > 1)
        {
            // Show a result of all rolls.
            string resultsString = string.Join(" + ", results);
            return new KumaResponse($"{resultsString} = {total}");
        }
            
        // Show just the total for single dice throws.
        return new KumaResponse($"{total}");
    }

    [Command("predict")]
    public static KumaResponse Predict(KumaRequest kumaRequest)
    {
        if (kumaRequest.CommandArgs.Length == 0) return new KumaResponse("You didn't ask a question.");

        string result = Rng.PickRandom(PredictionChoices);
        return new KumaResponse(result);
    }
}