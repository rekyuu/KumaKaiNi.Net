using System;

namespace KumaKaiNi.Core
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CommandAttribute : Attribute
    {
        public string[] Commands;

        public CommandAttribute(string command)
        {
            Commands = new string[] { command };
        }

        public CommandAttribute(string[] commands)
        {
            Commands = commands;
        }
    }
}
