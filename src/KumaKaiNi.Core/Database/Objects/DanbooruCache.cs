using System;
using System.Collections.Generic;
using System.Text;

namespace KumaKaiNi.Core
{
    public class DanbooruCache : DatabaseObject
    {
        public string FileUrl;
        public DateTime Expires;
        public RequestProtocol Protocol;
        public long ChannelId;

        public DanbooruCache() : base() { }
        public DanbooruCache(Dictionary<string, object> row) : base(row) { }
    }
}
