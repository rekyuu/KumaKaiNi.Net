using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace KumaKaiNi.Core
{
    public static class Logging
    {
        public static void LogToDatabase(Request request)
        {
            Log log = new Log()
            {
                Timestamp = DateTime.UtcNow,
                Protocol = request.Protocol,
                Message = request.Message,
                MessageId = request.MessageId,
                UserId = request.UserId,
                ChannelId = request.ChannelId
            };

            log.Insert();
        }
    }
}
