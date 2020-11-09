using System;
using System.Collections.Generic;
using System.Text;

namespace KumaKaiNi.Core
{
    public class Quote : DatabaseObject
    {
        public string Text;

        public Quote() : base() { }
        public Quote(Dictionary<string, object> row) : base(row) { }
    }
}
