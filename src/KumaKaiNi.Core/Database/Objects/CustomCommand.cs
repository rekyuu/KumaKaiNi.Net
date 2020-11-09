using System;
using System.Collections.Generic;
using System.Text;

namespace KumaKaiNi.Core
{
    public class CustomCommand : DatabaseObject
    {
        public string Command;
        public string Response;

        public CustomCommand() : base() { }
        public CustomCommand(Dictionary<string, object> row) : base(row) { }
    }
}
