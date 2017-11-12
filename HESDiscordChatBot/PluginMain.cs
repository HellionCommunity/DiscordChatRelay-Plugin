using Discord.WebSocket;
using HellionExtendedServer.Common.Plugins;
using HellionExtendedServer.Managers.Plugins;
using System;
using System.IO;
using System.Threading.Tasks;
using ZeroGravity.Network;
using ZeroGravity.Helpers;

namespace HESDiscordChatBot
{
    [Plugin(API = "1.0.0", Author = "generalwrex", Description = "Links Chat to/from discord via a bot", Name = "HesDiscordChatBot", Version = "1.0.0")]
    public class PluginMain : PluginBase
    {
        private static bool debugMode;
        private static ulong channelID;
        private static DiscordClient discordClient;

        public ThreadSafeDictionary<long, NetworkController.Client> Clients { get { return GetServer.NetworkController.clientList; } }

        public PluginMain()
        {
           

        }

        public override void Init(string modDirectory)
        {
            try
            {
                MyConfig.FileName = Path.Combine(modDirectory, "Config.xml");

                var config = new MyConfig();
                debugMode = config.Settings.DebugMode;
                channelID = config.Settings.MainChannelID;

                discordClient = new DiscordClient();

                DiscordClient.SocketClient.MessageReceived += SocketClient_MessageReceived;

                DiscordClient.Instance.Start();

                Console.WriteLine("HESDiscordChatBot - Bot Started");

                Console.WriteLine("HESDiscordChatBot - Registering Events:");
                // the text chat message
                GetServer.NetworkController.EventSystem.AddListener(typeof(TextChatMessage), new EventSystem.NetworkDataDelegate(this.TextChatMessageListener));
                Console.Write(" [ChatMessage]");
                //new player spawned
                GetServer.NetworkController.EventSystem.AddListener(typeof(PlayersOnServerRequest), new EventSystem.NetworkDataDelegate(this.PlayerOnServerListener));
                Console.Write(" [NewPlayerConnected]");
                //player spawned
                GetServer.NetworkController.EventSystem.AddListener(typeof(PlayerSpawnRequest), new EventSystem.NetworkDataDelegate(this.PlayerSpawnRequestListener));
                Console.Write(" [PlayerConnected]");
                //player has disconnected from the server
                GetServer.NetworkController.EventSystem.AddListener(typeof(LogOutRequest), new EventSystem.NetworkDataDelegate(this.LogOutRequestListener));

                GetServer.NetworkController.EventSystem.AddListener(typeof(PlayerRespawnRequest), new EventSystem.NetworkDataDelegate(this.RespawnRequestListener));
                Console.Write(" [LogOutRequest]");

                Console.WriteLine("HESDiscordChatBot - Events Registered!");
            }
            catch (Exception ex)
            {
                GetLogger.Warn(ex, "HESDiscordChatBot Initialization failed.");
            }
          
        }


        private Task SocketClient_MessageReceived(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            string outMsg = String.Empty;

            if (message != null)
            {
                // if the channel is the main channel
                if (message.Channel.Id == MyConfig.Instance.Settings.MainChannelID)
                {
                    // if the sender isn't this bot
                    if (message.Author.Id != MyConfig.Instance.Settings.BotClientID)
                    {
                        outMsg = $"Discord - {message.Author.Username}: {message.Content}";

                        byte[] guid = Guid.NewGuid().ToByteArray();

                        TextChatMessage textChatMessage = new TextChatMessage();

                        textChatMessage.GUID = BitConverter.ToInt64(guid, 0);
                        textChatMessage.Name = ("");
                        textChatMessage.MessageText = outMsg;

                        GetServer.NetworkController.SendToAllClients(textChatMessage, (textChatMessage).Sender);


                        if (debugMode)
                            Console.WriteLine($"-> Got Message From Discord: {outMsg}");
                    }
                }
            }

            return Task.Run(() => Console.WriteLine(!MyConfig.Instance.Settings.PrintDiscordChatToConsole ? String.Empty : outMsg));
        }


        private async void RespawnRequestListener(NetworkData data)
        {
            if (debugMode)
                Console.WriteLine($"<- Sending Respawn Message To Discord");

            PlayerRespawnRequest request = data as PlayerRespawnRequest;

            var player = Clients[request.Sender].Player;

            if (player == null)
                return;

            string outMsg = $"Player '{player.Name}' is respawning on the game server.";

            await (DiscordClient.SocketClient.GetChannel(MyConfig.Instance.Settings.MainChannelID) as Discord.IMessageChannel).SendMessageAsync(outMsg);
        }

        public async void PlayerOnServerListener(NetworkData data)
        {
            if (debugMode)
                Console.WriteLine($"<- Sending New Player Message To Discord");

            PlayersOnServerRequest request = data as PlayersOnServerRequest;

            var player = Clients[request.Sender].Player;

            if (player == null)
                return;

            string outMsg = $"A new player '{player.Name}' has connected to the game server.";

            await (DiscordClient.SocketClient.GetChannel(MyConfig.Instance.Settings.MainChannelID) as Discord.IMessageChannel).SendMessageAsync(outMsg);


        }

        public async void LogOutRequestListener(NetworkData data)
        {
            if (debugMode)
                Console.WriteLine($"<- Sending Disconnect Message To Discord");

            LogOutRequest request = data as LogOutRequest;

            var player = Clients[request.Sender].Player;

            if (player == null)
                return;

            string outMsg = $"{player.Name} disconnected from the game server.";
            await (DiscordClient.SocketClient.GetChannel(MyConfig.Instance.Settings.MainChannelID) as Discord.IMessageChannel).SendMessageAsync(outMsg);
        }

        public async void PlayerSpawnRequestListener(NetworkData data)
        {
            if (debugMode)
                Console.WriteLine($"<- Sending Player Spawn To Discord");

            PlayerSpawnRequest request = data as PlayerSpawnRequest;

            var player = Clients[request.Sender].Player;

            if (player == null)
                return;

            string outMsg = $"{player.Name} connected to the game server.";
            await (DiscordClient.SocketClient.GetChannel(MyConfig.Instance.Settings.MainChannelID) as Discord.IMessageChannel).SendMessageAsync(outMsg);
        }

        public async void TextChatMessageListener(NetworkData data)
        {
            if (debugMode)
                Console.WriteLine($"<- Sending Message To Discord");

            TextChatMessage textChatMessage = data as TextChatMessage;

            string outMsg = $"Game Server - {textChatMessage.Name}: {textChatMessage.MessageText} ";

            await (DiscordClient.SocketClient.GetChannel(MyConfig.Instance.Settings.MainChannelID) as Discord.IMessageChannel).SendMessageAsync(outMsg);

            if (MyConfig.Instance.Settings.PrintDiscordChatToConsole)
                Console.WriteLine(outMsg);


        }
    }
}