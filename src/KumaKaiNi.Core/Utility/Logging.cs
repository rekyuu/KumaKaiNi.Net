using System;

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

            string responseString = response.Message;
            if (responseString == "" && response.Image != null) responseString = $"{response.Image.Referrer}\n{response.Image.Description}\n{response.Image.Url}\n{response.Image.Source}";

            Log responseLog = new Log()
            {
                Timestamp = DateTime.UtcNow,
                Protocol = request.Protocol,
                Message = responseString,
                Username = "KumaKaiNi",
                ChannelId = request.ChannelId,
                Private = request.ChannelIsPrivate
            };

            try
            {
                if (requestLog.Message != "")
                {
                    requestLog.Insert();
                    Console.WriteLine($"{requestLog.Timestamp} [{request.Protocol}] {request.Username}: {request.Message}");
                }

                if (responseLog.Message != "")
                {
                    responseLog.Insert();
                    Console.WriteLine($"{responseLog.Timestamp} [{request.Protocol}] KumaKaiNi: {responseLog.Message}");
                }
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
