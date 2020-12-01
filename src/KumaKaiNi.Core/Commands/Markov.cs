﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KumaKaiNi.Core
{
    public static class Markov
    {
        [Command("markov")]
        public static Response MarkovCommand(Request request)
        {
            WherePredicate logProtocol = new WherePredicate()
            {
                Source = "protocol",
                Comparitor = "= any",
                Target = new string[] { "Twitch", "Discord" }
            };

            WherePredicate logChannels = new WherePredicate()
            {
                Source = "channel_id",
                Comparitor = "= any",
                Target = new long[] { 0, 214268737887404042 }
            };

            WherePredicate excludeKuma = new WherePredicate()
            {
                Source = "username",
                Comparitor = "<>",
                Target = "KumaKaiNi"
            };

            WherePredicate excludeCommands = new WherePredicate()
            {
                Source = "message",
                Comparitor = "NOT LIKE",
                Target = "!%"
            };

            WherePredicate excludeLinks = new WherePredicate()
            {
                Source = "message",
                Comparitor = "!~*",
                Target = ".*((([A-Za-z]{3,9}:(?:\\/\\/)?)(?:[-;:&=\\+\\$,\\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\\+\\$,\\w]+@)[A-Za-z0-9.-]+)((?:\\/[\\+~%\\/.\\w-_]*)?\\??(?:[-\\+=&;%@.\\w_]*)#?(?:[\\w]*))?).*"
            };

            List<Log> logs = Database.GetMany<Log>(new WherePredicate[] { logProtocol, logChannels, excludeKuma, excludeCommands, excludeLinks }, 1000);

            string[] lines = new string[logs.Count];
            for (int i = 0; i < logs.Count; i++)
            {
                lines[i] = logs[i].Message;
            }

            string markovResponse = GetMarkovResponse(lines);
            return new Response(markovResponse);
        }

        private static string GetMarkovResponse(string[] lines)
        {
            Dictionary<string, List<string>> wordDictionary = CreateWordDictionary(lines);
            string startWord = RNG.PickRandom(wordDictionary.Keys.ToArray());

            return GenerateMarkov(wordDictionary, startWord);
        }

        private static Dictionary<string, List<string>> CreateWordDictionary(string[] lines)
        {
            Dictionary<string, List<string>> wordDictionary = new Dictionary<string, List<string>>();
            string[] words = string.Join(" \n ", lines).Split(" ");

            while (words.Count() > 1)
            {
                string word1 = CleanEmoteWord(words[0]);
                string word2 = CleanEmoteWord(words[1]);
                words = words[1..];

                switch (word1)
                {
                    case "\n":
                        break;
                    default:
                        List<string> wordValues = wordDictionary.GetValueOrDefault(word1, new List<string>());
                        wordValues.Add(word2);
                        wordDictionary[word1] = wordValues;
                        break;
                }
            }

            return wordDictionary;
        }

        private static string CleanEmoteWord(string word)
        {
            MatchCollection matches = Regex.Matches(word, @"<:(?<emote>.+):\d{18}>");

            if (matches.Count == 0) return word;
            else return matches[0].Groups["emote"].Value;
        }

        private static string GenerateMarkov(Dictionary<string, List<string>> wordDictionary, string startWord)
        {
            StringBuilder sb = new StringBuilder(startWord);

            string lastWord = startWord;
            while (true)
            {
                string nextWord = RNG.PickRandom(wordDictionary[lastWord]);
                if (nextWord == "\n") break;

                sb.Append($" {nextWord}");
                lastWord = nextWord;
            }

            return sb.ToString();
        }
    }
}
