using System.Collections.Generic;

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
