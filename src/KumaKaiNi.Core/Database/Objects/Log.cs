using System;
using System.Collections.Generic;

namespace KumaKaiNi.Core
{
    public class Log : DatabaseObject
    {
        public DateTime Timestamp;
        public RequestProtocol Protocol;
        public string Message;
        public long MessageId;
        public string Username;
        public long ChannelId;
        public bool Private;

        public Log() : base() { }
        public Log(Dictionary<string, object> row) : base(row) { }
    }
}
