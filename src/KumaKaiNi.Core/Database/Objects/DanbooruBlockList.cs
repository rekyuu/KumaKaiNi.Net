using System.Collections.Generic;

namespace KumaKaiNi.Core
{
    public class DanbooruBlockList : DatabaseObject
    {
        public string Tag;

        public DanbooruBlockList() { }
        
        public DanbooruBlockList(Dictionary<string, object> row) : base(row) { }
    }
}
