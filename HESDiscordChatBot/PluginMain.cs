using Discord.WebSocket;
using HellionExtendedServer.Common.Plugins;
using HellionExtendedServer.Managers.Plugins;
using System;
using System.IO;
using System.Threading.Tasks;
using ZeroGravity.Network;

namespace HESDiscordChatBot
{
    [Plugin(API = "1.0.0", Author = "generalwrex", Description = "Links Chat to/from discord via a bot", Name = "HesDiscordChatBot", Version = "1.0.0")]
    public class PluginMain : PluginBase
    {
        private static bool debugMode;
        private static ulong channelID;
        private static DiscordClient discordClient;

        public PluginMain()
        {
          
        }

        public override void Init(string modDirectory)
        {
            MyConfig.FileName = Path.Combine(modDirectory, "Config.xml");

            var config = new MyConfig();
            debugMode = config.Settings.DebugMode;
            channelID = config.Settings.MainChannelID;

            discordClient = new DiscordClient();

            SetupServerEventHandlers();
            SetupDiscordEventHandlers();

            DiscordClient.Instance.Start();

            Console.WriteLine("HESDiscordChatBot - Bot Started");
        }

        private void SetupDiscordEventHandlers()
        {
            DiscordClient.SocketClient.MessageReceived += SocketClient_MessageReceived;
        }

        private void SetupServerEventHandlers()
        {
            // the text chat message
            GetServer.NetworkController.EventSystem.AddListener(typeof(TextChatMessage), new EventSystem.NetworkDataDelegate(this.TextChatMessageListener));

            //new player spawned
            GetServer.NetworkController.EventSystem.AddListener(typeof(PlayersOnServerRequest), new EventSystem.NetworkDataDelegate(this.PlayerOnServerListener));

            //player spawned
            GetServer.NetworkController.EventSystem.AddListener(typeof(PlayerSpawnRequest), new EventSystem.NetworkDataDelegate(this.PlayerSpawnRequestListener));

            //player has disconnected from the server
            GetServer.NetworkController.EventSystem.AddListener(typeof(LogOutRequest), new EventSystem.NetworkDataDelegate(this.LogOutRequestListener));
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
                    // if the sender isn't a bot
                    if (!message.Author.IsBot)
                    {
                        outMsg = $"Discord - {message.Author.Username}: {message.Content}";

                        byte[] guid = Guid.NewGuid().ToByteArray();

                        TextChatMessage textChatMessage = new TextChatMessage();

                        textChatMessage.GUID = BitConverter.ToInt64(guid, 0);
                        textChatMessage.Name = ("");
                        textChatMessage.MessageText = outMsg;

                        GetServer.NetworkController.SendToAllClients(textChatMessage, (textChatMessage).Sender);
                      
                    }
                }
            }

            return Task.Run(() => Console.WriteLine(!MyConfig.Instance.Settings.PrintDiscordChatToConsole ? String.Empty : outMsg));
        }

        private void PlayerOnServerListener(NetworkData data)
        {
            PlayersOnServerRequest request = data as PlayersOnServerRequest;
            if (request == null)
                return;

            var player = GetServer.GetPlayer(request.Sender);
            string outMsg = $"A new player '{player.Name}' has connected to the game server.";
            DiscordClient.SendMessageToChannel(MyConfig.Instance.Settings.MainChannelID, outMsg);
        }

        private void LogOutRequestListener(NetworkData data)
        {
            LogOutRequest request = data as LogOutRequest;
            if (request == null)
                return;

            var player = GetServer.GetPlayer(request.Sender);

            string outMsg = $"{player.Name} disconnected from the game server.";
            DiscordClient.SendMessageToChannel(MyConfig.Instance.Settings.MainChannelID, outMsg);
        }

        private void PlayerSpawnRequestListener(NetworkData data)
        {
            PlayerSpawnRequest playerSpawnRequest = data as PlayerSpawnRequest;
            if (playerSpawnRequest == null)
                return;

            var player = GetServer.GetPlayer(playerSpawnRequest.Sender);

            string outMsg = $"{player.Name} connected to the game server.";
            DiscordClient.SendMessageToChannel(MyConfig.Instance.Settings.MainChannelID, outMsg);
        }

        public void TextChatMessageListener(NetworkData data)
        {
            TextChatMessage textChatMessage = data as TextChatMessage;

            string outMsg = $"Game Server - {textChatMessage.Name}: {textChatMessage.MessageText} ";

            DiscordClient.SendMessageToChannel(channelID, outMsg);

            if (MyConfig.Instance.Settings.PrintDiscordChatToConsole)
                Console.WriteLine(outMsg);
        }
    }
}