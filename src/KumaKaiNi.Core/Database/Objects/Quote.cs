using System.Collections.Generic;

namespace KumaKaiNi.Core
{
    public class Quote : DatabaseObject
    {
        public string Text;

        public Quote() { }
        
        public Quote(Dictionary<string, object> row) : base(row) { }
    }
}
