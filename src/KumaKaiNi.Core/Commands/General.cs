using System;

namespace KumaKaiNi.Core
{
    public static class General
    {
        [Command("help")]
        public static Response Help()
        {
            return new Response("I'll write this later probably lmao");
        }

        [Command("kuma")]
        [Command("ping")]
        public static Response Kuma()
        {
            string reply = RNG.PickRandom(new[] { "Kuma?", "Kuma~!", "Kuma...", "Kuma!!", "Kuma.", "Kuma...?" });

            return new Response(reply);
        }

        [Command("say")]
        public static Response Say(Request request)
        {
            if (request.CommandArgs.Length == 0) return new Response();
            return new Response(string.Join(" ", request.CommandArgs));
        }

        [Phrase(new string[] { "hi", "hello", "hey", "sup" })]
        public static Response Hello()
        {
            string reply = RNG.PickRandom(new string[] { "sup", "yo", "ay", "hi", "wassup" });

            return new Response(reply);
        }

        [Phrase(new string[] { "ty kuma", "thanks kuma", "thank you kuma" })]
        public static Response Thanks()
        {
            string reply = RNG.PickRandom(new string[] { "np", "don't mention it", "anytime", "sure thing", "ye whateva" });

            return new Response(reply);
        }

        [Phrase("same")]
        public static Response Same()
        {
            if (RNG.OneTo(25)) return new Response("same");
            else return new Response();
        }

        [Command("moon")]
        public static Response MoonPhase()
        {
            DateTime date = DateTime.UtcNow;
            (int year, int month, int day) = (date.Year, date.Month, date.Day);

            if (month <= 2)
            {
                year -= 1;
                month += 12;
            }

            int a = (int)(year / 100f);
            int b = (int)(a / 4f);
            int c = 2 - a + b;
            int e = (int)(365.25f * (year + 4716f));
            int f = (int)(30.6001f * (month + 1f));

            float cycleLength = 29.53f;
            float julianDate = c + day + e + f - 1524.5f;
            float daysSinceNewMoon = (int)(julianDate - 2451549.5f);
            float newMoons = daysSinceNewMoon / cycleLength;
            int d = (int)((newMoons - (int)newMoons) * cycleLength);

            if (d == 29) return new Response("New Moon");
            else if (d >= 23) return new Response("Waning Crescent");
            else if (d == 22) return new Response("Last Quarter");
            else if (d >= 16) return new Response("Waning Gibbous");
            else if (d == 15) return new Response("Full Moon");
            else if (d >= 9) return new Response("Waxing Gibbous");
            else if (d == 8) return new Response("First Quarter");
            else if (d >= 2) return new Response("Waxing Crescent");
            else if (d == 1) return new Response("New Moon");
            else return new Response("N̸̛̹̩͈͖̭̤͚̠̘̝̮͓͈̻̾̈̋̇͊̀̔̚͠͝e̷̡̧̼̩̰̼̞̖͙̮̙̰̳͑̇̑̽͆͆̇̍͘͠w̷̞̦̪̑̎̆̒ ̸̛̩͇̹̯̠̊͆̊Ḿ̵̲͕͔̼̘͙͍͇ͅơ̷̧̙͈͍̻̯̬͔̈́̐̂͌̏̚͘͜o̴̢͕̫̱̪̬̤̱̳͈̩̤̐̈͋̎̅́̿̏̊̕n̶̰̼̯̼͇͕̥̭̞̖͖̾̎́̄͆͂͋̽͌ͅ");
        }
    }
}
