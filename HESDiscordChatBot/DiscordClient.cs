﻿using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HESDiscordChatBot
{
    public class DiscordClient
    {
        public static DiscordSocketClient SocketClient = new DiscordSocketClient();
        public static DiscordClient Instance;

        private static bool debugMode;

        public DiscordClient()
        {
            Instance = this;

            debugMode = MyConfig.Instance.Settings.DebugMode;

            Console.WriteLine(!debugMode ? String.Empty : "HESDiscordChatBot - Bot Client Constructed");
        }

        public void Start()
        {
            var serverThread = new Thread(ThreadStart);
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        private void ThreadStart()
            => StartBotAsync().GetAwaiter().GetResult();

        private async Task StartBotAsync()
        {
            try
            {
                Console.WriteLine("HESDiscordChatBot - Bot Connecting");
               
                SocketClient.Connected += _client_Connected;
                SocketClient.Log += _client_Log; 
                SocketClient.LoggedIn += _client_LoggedIn;

                try
                {
                    await SocketClient.LoginAsync(TokenType.Bot, MyConfig.Instance.Settings.DiscordToken);
                }
                catch { }
                try
                {
                    await SocketClient.StartAsync();
                }
                catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HESDiscordChatBot - Start Exception: " + ex.ToString());
            }
        }

        private Task _client_LoggedIn()
        {
            return Task.Run(() => Console.WriteLine(!debugMode ? String.Empty : "HESDiscordChatBot - Logged In"));
        }

        private Task _client_Connected()
        {
            return Task.Run(() => Console.WriteLine(!debugMode ? String.Empty : "HESDiscordChatBot - Connected"));
        }

        private Task _client_Log(LogMessage arg)
        {
            return Task.Run(() => Console.WriteLine(MyConfig.Instance.Settings.PrintDiscordLogToConsole ? arg.Message : String.Empty));
        }

        public static void SendMessageToChannel(ulong channelID, string message)
        {
            (SocketClient.GetChannel(channelID) as IMessageChannel).SendMessageAsync(message);
        }
                 
    }
}