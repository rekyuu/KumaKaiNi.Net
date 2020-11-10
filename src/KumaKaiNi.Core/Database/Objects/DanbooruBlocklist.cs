using System.Collections.Generic;

namespace KumaKaiNi.Core
{
    public class DanbooruBlocklist : DatabaseObject
    {
        public string Tag;

        public DanbooruBlocklist() : base() { }
        public DanbooruBlocklist(Dictionary<string, object> row) : base(row) { }
    }
}
