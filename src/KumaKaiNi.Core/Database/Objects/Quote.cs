using System.Collections.Generic;

namespace KumaKaiNi.Core
{
    public class Quote : DatabaseObject
    {
        public string Text;

        public Quote() : base() { }
        public Quote(Dictionary<string, object> row) : base(row) { }
    }
}
