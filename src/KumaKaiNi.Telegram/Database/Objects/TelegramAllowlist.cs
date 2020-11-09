using KumaKaiNi.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace KumaKaiNi.Telegram
{
    public class TelegramAllowlist : DatabaseObject
    {
        public long ChannelId;
        public bool Approved;
        public int Warnings;

        public TelegramAllowlist() : base() { }
        public TelegramAllowlist(Dictionary<string, object> row) : base(row) { }
    }
}
