using System;

namespace KumaKaiNi.Core
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PhraseAttribute : Attribute
    {
        public string[] Phrases;

        public PhraseAttribute(string command)
        {
            Phrases = new string[] { command };
        }

        public PhraseAttribute(string[] commands)
        {
            Phrases = commands;
        }
    }
}
