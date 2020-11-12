using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace KumaKaiNi.Core
{
    public static class Randomized
    {
        [Command("coin")]
        [Command("flip")]
        public static Response CoinFlip()
        {
            string result = RNG.PickRandom(new[] { "Heads.", "Tails." });
            return new Response(result);
        }

        [Command("pick")]
        [Command("choose")]
        public static Response PickOne(Request request)
        {
            if (request.CommandArgs.Length < 2) return new Response("Usage: !pick option1, option2, [option3, ...]");

            string[] commaSplitArgs = string.Join(" ", request.CommandArgs).Split(", ");

            string result = RNG.PickRandom(commaSplitArgs);
            return new Response(result);
        }

        [Command("roll")]
        [Command("dice")]
        public static Response RollDice(Request request)
        {
            Random rng = new Random();

            // User supplied an argument (ie, !roll 4 or roll 4d6)
            if (request.CommandArgs.Length > 0)
            {
                string[] args = request.CommandArgs[0].Split('d');
                int diceAmount = 0;
                int diceSides = 0;

                try
                {
                    // Parse the xdy format.
                    if (args.Length >= 2 && args[0] != "" && args[1] != "")
                    {
                        diceAmount = Convert.ToInt32(args[0]);
                        diceSides = Convert.ToInt32(args[1]);
                    }
                    // Parse the number only format.
                    else if (args.Length == 1 && args[0] != "")
                    {
                        diceAmount = 1;
                        diceSides = Convert.ToInt32(args[0]);
                    }
                }
                catch
                {
                    // Assume the argumentsd given were not proper and return a help message.
                    return new Response("Usage: !roll\n       !roll {dice sides}\n       !roll {dice amount}d{dice sides}\n");
                }

                if (diceAmount > 0 && diceSides > 2)
                {
                    // Get the results and the sum of the rolls.
                    int total = 0;
                    List<int> results = new List<int>();
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
                        return new Response($"{resultsString} = {total}");
                    }
                    // Show just the total for single dice throws.
                    else return new Response($"{total}");
                }
                else return new Response("The dice amount must be 1 or more, and the dice sides must be 3 or more.");
            }
            else
            {
                // Roll a d6.
                int result = rng.Next(1, 7);
                return new Response(result.ToString());
            }
        }

        [Command("predict")]
        public static Response Predict(Request request)
        {
            if (request.CommandArgs.Length == 0) return new Response("You didn't ask a question.");

            string[] predictions = new string[]
            {
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
            };

            string result = RNG.PickRandom(predictions);
            return new Response(result);
        }

        [Command("gdq")]
        public static Response GDQ()
        {
            using HttpClient client = new HttpClient();
            HttpResponseMessage request = client.GetAsync("https://taskinoz.com/gdq/api/").Result;
            string result = request.Content.ReadAsStringAsync().Result;

            return new Response(result);
        }
    }
}
