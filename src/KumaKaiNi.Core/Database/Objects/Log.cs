using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace KumaKaiNi.Core
{
    public class Log : DatabaseObject
    {
        public DateTime Timestamp;
        public RequestProtocol Protocol;
        public string Message;
        public string MessageId;
        public string UserId;
        public string ChannelId;

        public Log() : base() { }
        public Log(Dictionary<string, object> row) : base(row) { }
    }
}
