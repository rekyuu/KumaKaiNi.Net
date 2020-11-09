using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KumaKaiNi.Core
{
    public class Request
    {
        public string Message;
        public string MessageId;

        public string Username;
        public string UserId;
        public bool UserIsAdmin;

        public RequestProtocol Protocol;
        public string ChannelId;
        public bool ChannelIsPrivate;
        public bool ChannelIsNSFW;

        public bool IsCommand = false;
        public string Command;
        public string[] CommandArgs;

        public Request(RequestProtocol protocol, string message, object messageId, string username, object userId, object channelId, bool userIsAdmin = false, bool channelIsPrivate = false, bool channelIsNSFW = false)
        {
            Protocol = protocol;
            Message = message;
            MessageId = messageId.ToString();

            Username = username;
            UserId = userId.ToString();
            UserIsAdmin = userIsAdmin;

            ChannelId = channelId.ToString();
            ChannelIsPrivate = channelIsPrivate;
            ChannelIsNSFW = channelIsNSFW;

            if (message[0] == '!')
            {
                string[] messageContents = message.Split(' ');

                IsCommand = true;
                Command = messageContents[0].Substring(1, messageContents[0].Length - 1);
                CommandArgs = messageContents.Skip(1).ToArray();
            }
        }
    }
}
