using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace KumaKaiNi.Core
{
    public static class Logging
    {
        public static void LogToDatabase(Request request, Response response)
        {
            Log requestLog = new Log()
            {
                Timestamp = DateTime.UtcNow,
                Protocol = request.Protocol,
                Message = request.Message,
                MessageId = request.MessageId,
                Username = request.Username,
                ChannelId = request.ChannelId,
                Private = request.ChannelIsPrivate
            };

            Log responseLog = new Log()
            {
                Timestamp = DateTime.UtcNow,
                Protocol = request.Protocol,
                Message = response.Message,
                Username = "KumaKaiNi",
                ChannelId = request.ChannelId,
                Private = request.ChannelIsPrivate
            };

            try
            {
                if (requestLog.Message != "") requestLog.Insert();
                if (responseLog.Message != "") responseLog.Insert();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        public static void LogException(Exception ex)
        {
            Error error = new Error(ex);
            if (error.TableExists()) error.Insert();

            Console.WriteLine($"{error.Timestamp} [{error.Source}] Exception: {ex.Message}");
        }
    }
}
