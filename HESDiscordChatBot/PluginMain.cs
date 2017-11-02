using Discord;
using Discord.WebSocket;
using HellionExtendedServer.Common.Plugins;
using HellionExtendedServer.Managers.Plugins;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ZeroGravity.Network;
using ZeroGravity.Objects;

namespace HESDiscordChatBot
{
    public class MyConfig
    {
        public string DiscordToken;
        public ulong MainChannelID;
    }

    [Plugin(API = "1.0.0", Author = "generalwrex",
        Description = "Links Chat to/from discord via a bot",
        Name = "HesDiscordChatBot", Version = "1.0.0")]
    public class PluginMain : PluginBase
    {
        private static DiscordSocketClient _client;

        private static MyConfig myConfig = new MyConfig();

        public PluginMain()
        {
            
        }

        public void LoadConfig(string path)
        {
            using (StreamReader file = File.OpenText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                myConfig = (MyConfig)serializer.Deserialize(file, typeof(MyConfig));
            }
        }

        public void SaveConfig(string path)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(path))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, myConfig);
            }
        }

        private void ThreadStart()
     => StartBotAsync().GetAwaiter().GetResult();

        public override void Init(string modDirectory)
        {
            string path = Path.Combine(modDirectory, "Config.json");

            if (File.Exists(path))
                LoadConfig(path);

            GetServer.NetworkController.EventSystem.AddListener(typeof(TextChatMessage), new EventSystem.NetworkDataDelegate(this.TextChatMessageListener));
            GetServer.NetworkController.EventSystem.AddListener(typeof(PlayerSpawnRequest), new EventSystem.NetworkDataDelegate(this.PlayerSpawnRequest));

            var serverThread = new Thread(this.ThreadStart);
            serverThread.IsBackground = true;
            serverThread.Start();
            Console.WriteLine("HESDiscordChatBot - Bot Started");
        }

        private void PlayerSpawnRequest(NetworkData data)
        {
            PlayerSpawnRequest playerSpawnRequest = data as PlayerSpawnRequest;
            if (playerSpawnRequest == null)
                return;

            var player = GetServer.GetPlayer(playerSpawnRequest.Sender);

            string outMsg = $"{player.Name} connected to the game server.";
            SendMessageToGeneral(outMsg);
        }

        public async Task StartBotAsync()
        {
            try
            {
                Console.WriteLine("HESDiscordChatBot - Bot Connecting");

                _client = new DiscordSocketClient();

                _client.Connected += _client_Connected;
                _client.Log += _client_Log;
                _client.MessageReceived += MessageReceivedFromDiscord;
                _client.LoggedIn += _client_LoggedIn;

                try
                {
                    await _client.LoginAsync(TokenType.Bot, myConfig.DiscordToken);
                }
                catch { }
                try
                {
                    await _client.StartAsync();
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
            return Task.Run(() => Console.WriteLine("HESDiscordChatBot - Logged In"));
        }

        private Task _client_Connected()
        {
            return Task.Run(() => Console.WriteLine("HESDiscordChatBot - Connected"));
        }

        private Task _client_Log(LogMessage arg)
        {
            return Task.Run(() => Console.WriteLine(arg.Message));
        }

        public override void OnCommand(Player p, string command, string[] args)
        {
            if (command.ToLower() == "discordinfo")
            {
                Console.WriteLine("Discord Info");
                Console.WriteLine("Connection:" + _client.ConnectionState.ToString());
            }

            if (command.ToLower() == "msgdiscord")
            {
                SendMessageToGeneral(args.ToString());
                Console.WriteLine("Sent Message to Discord");
            }
        }

        private async Task MessageReceivedFromDiscord(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;

            if (message == null) return;

            if (message.Channel.Id == myConfig.MainChannelID)
            {
                if (!message.Author.IsBot)
                {
                    string outMsg = $"Discord [#general] - {message.Author.Username}: {message.Content}";
                    GetPluginHelper.SendMessageToServer(outMsg);
                    Console.WriteLine(outMsg);
                }
            }
        }

        public void TextChatMessageListener(NetworkData data)
        {
            TextChatMessage textChatMessage = data as TextChatMessage;

            string outMsg = $"HES Server - {textChatMessage.Name}: {textChatMessage.MessageText} ";

            SendMessageToGeneral(outMsg);

            Console.WriteLine(outMsg);
        }

        public static void SendMessageToGeneral(string message)
                  => (_client.GetChannel(myConfig.MainChannelID) as IMessageChannel).SendMessageAsync(message);
    }
}