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
            string reply = Rng.PickRandom(new[] { "Kuma?", "Kuma~!", "Kuma...", "Kuma!!", "Kuma.", "Kuma...?" });

            return new Response(reply);
        }

        [Command("say")]
        public static Response Say(Request request)
        {
            if (request.CommandArgs.Length == 0) return new Response();
            return new Response(string.Join(" ", request.CommandArgs));
        }

        [Phrase(new[] { "hi", "hello", "hey", "sup" })]
        public static Response Hello()
        {
            string reply = Rng.PickRandom(new[] { "sup", "yo", "ay", "hi", "wassup" });

            if (Rng.OneTo(25)) return new Response(reply);
            else return new Response();
        }

        [Phrase(new[] { "ty kuma", "thanks kuma", "thank you kuma" })]
        public static Response Thanks()
        {
            string reply = Rng.PickRandom(new[] { "np", "don't mention it", "anytime", "sure thing", "ye whateva" });

            return new Response(reply);
        }

        [Phrase("same")]
        public static Response Same()
        {
            if (Rng.OneTo(25)) return new Response("same");
            else return new Response();
        }

        [Command("projection")]
        public static Response Projection()
        {
            return new Response("Psychological projection is a theory in psychology in which humans defend themselves against their own unpleasant impulses by denying their existence while attributing them to others. For example, a person who is rude may constantly accuse other people of being rude. It can take the form of blame shifting.");
        }

        [Command("moon")]
        public static Response MoonPhase()
        {
            int phase = Helpers.GetMoonPhase(DateTime.UtcNow);

            if (phase == 29) return new Response("New Moon");
            else if (phase >= 23) return new Response("Waning Crescent");
            else if (phase == 22) return new Response("Last Quarter");
            else if (phase >= 16) return new Response("Waning Gibbous");
            else if (phase == 15) return new Response("Full Moon");
            else if (phase >= 9) return new Response("Waxing Gibbous");
            else if (phase == 8) return new Response("First Quarter");
            else if (phase >= 2) return new Response("Waxing Crescent");
            else if (phase == 1) return new Response("New Moon");
            else return new Response("N̸̛̹̩͈͖̭̤͚̠̘̝̮͓͈̻̾̈̋̇͊̀̔̚͠͝e̷̡̧̼̩̰̼̞̖͙̮̙̰̳͑̇̑̽͆͆̇̍͘͠w̷̞̦̪̑̎̆̒ ̸̛̩͇̹̯̠̊͆̊Ḿ̵̲͕͔̼̘͙͍͇ͅơ̷̧̙͈͍̻̯̬͔̈́̐̂͌̏̚͘͜o̴̢͕̫̱̪̬̤̱̳͈̩̤̐̈͋̎̅́̿̏̊̕n̶̰̼̯̼͇͕̥̭̞̖͖̾̎́̄͆͂͋̽͌ͅ");
        }
    }
}
