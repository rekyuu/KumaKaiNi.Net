﻿using KumaKaiNi.Core;
using System;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using System.Timers;

namespace KumaKaiNi.Twitch
{
    public class App
    {
        private KumaClient _kuma;
        private TwitchClient _twitch;

        public App()
        {
            _kuma = new KumaClient();

            ClientOptions clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };

            WebSocketClient webSocketClient = new WebSocketClient(clientOptions);
            _twitch = new TwitchClient(webSocketClient);

            _twitch.OnLog += Log;
            _twitch.OnConnected += Connected;
            _twitch.OnJoinedChannel += ChannelJoined;
            _twitch.OnMessageReceived += MessageReceived;
            _twitch.OnWhisperReceived += WhisperReceived;
        }

        public async Task Start()
        {
            ConnectionCredentials credentials = new("KumaKaiNi", BotConfig.TwitchAccessToken);

            _twitch.Initialize(credentials, "rekyuus");
            Reconnect();

            Timer reconnectTimer = new Timer(24 * 60 * 60 * 1000);
            reconnectTimer.Elapsed += new ElapsedEventHandler(ReconnectHandler);
            reconnectTimer.Start();

            await Task.Delay(-1);
        }

        private void ReconnectHandler(object source, ElapsedEventArgs e)
        {
            Reconnect();
        }

        private void Reconnect()
        {            
            if (_twitch.IsConnected) _twitch.Disconnect();
            _twitch.Connect();
        }

        private void Log(object sender, OnLogArgs e)
        {
            // Console.WriteLine($"[{DateTime.UtcNow}] {e.BotUsername}: {e.Data}");
        }

        private void Connected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine($"{DateTime.UtcNow} [KumaKaiNi.Twitch] Connected to {e.AutoJoinChannel}");
        }

        private void ChannelJoined(object sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine($"{DateTime.UtcNow} [KumaKaiNi.Twitch] Joined {e.Channel}");
        }

        private void MessageReceived(object sender, OnMessageReceivedArgs message)
        {
            try
            {
                if (message.ChatMessage.Username == "kumakaini") return;

                bool isAdmin = message.ChatMessage.Username == "rekyuus";
                bool isModerator = message.ChatMessage.IsModerator;

                UserAuthority authority = UserAuthority.User;
                if (isAdmin) authority = UserAuthority.Admin;
                else if (isModerator) authority = UserAuthority.Moderator;

                Request request = new Request()
                {
                    Message = message.ChatMessage.Message,
                    MessageId = 0,
                    Username = message.ChatMessage.Username,
                    Authority = authority,
                    Protocol = RequestProtocol.Twitch,
                    ChannelId = 0,
                    ChannelIsPrivate = false,
                    ChannelIsNsfw = false,
                };
                Response response = _kuma.GetResponse(request);

                if (response.Message != "")
                {
                    if (response.Message[0] == '/') response.Message = response.Message[1..];
                    _twitch.SendMessage(message.ChatMessage.Channel, response.Message);
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
            }
        }

        private void WhisperReceived(object sender, OnWhisperReceivedArgs message)
        {
            try
            {
                if (message.WhisperMessage.Username == "kumakaini") return;

                bool isAdmin = message.WhisperMessage.Username == "rekyuus";

                UserAuthority authority = UserAuthority.User;
                if (isAdmin) authority = UserAuthority.Admin;

                Request request = new Request()
                {
                    Message = message.WhisperMessage.Message,
                    MessageId = 0,
                    Username = message.WhisperMessage.Username,
                    Authority = authority,
                    Protocol = RequestProtocol.Twitch,
                    ChannelId = 0,
                    ChannelIsPrivate = true,
                    ChannelIsNsfw = false,
                };
                Response response = _kuma.GetResponse(request);

                if (response.Message != "")
                {
                    if (response.Message[0] == '/') response.Message = response.Message[1..];
                    _twitch.SendWhisper(message.WhisperMessage.Username, response.Message);
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
            }
        }
    }
}
