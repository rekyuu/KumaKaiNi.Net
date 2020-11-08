using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KumaKaiNi.Core
{
    public class Request
    {
        public string Message;
        public bool ChannelIsPrivate;
        public bool ChannelIsNSFW;
        public bool UserIsModerator;

        public bool IsCommand = false;
        public string Command;
        public string[] CommandArgs;

        public Request(string message, bool channelIsPrivate = false, bool channelIsNSFW = false, bool userIsModerator = false)
        {
            Message = message;
            ChannelIsPrivate = channelIsPrivate;
            ChannelIsNSFW = channelIsNSFW;
            UserIsModerator = userIsModerator;

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
