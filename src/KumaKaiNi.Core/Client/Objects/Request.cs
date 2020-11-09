using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KumaKaiNi.Core
{
    public class Request
    {
        public string Message;
        public long MessageId;

        public string Username;
        public bool UserIsAdmin;

        public RequestProtocol Protocol;
        public long ChannelId;
        public bool ChannelIsPrivate;
        public bool ChannelIsNSFW;

        public bool IsCommand = false;
        public string Command;
        public string[] CommandArgs;

        public void Parse()
        {
            if (Message != "" && Message[0] == '!')
            {
                string[] messageContents = Message.Split(' ');

                IsCommand = true;
                Command = messageContents[0][1..];
                CommandArgs = messageContents.Skip(1).ToArray();
            }
        }
    }
}
