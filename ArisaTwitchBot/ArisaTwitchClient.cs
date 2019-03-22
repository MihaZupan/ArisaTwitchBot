using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchLib.Api.Core;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Users;
using TwitchLib.Api.Helix;
using Stream = TwitchLib.Api.Helix.Models.Streams.Stream;

using ArisaTwitchBot.Services;

namespace ArisaTwitchBot
{
    public class ArisaTwitchClient
    {
        public readonly TwitchClient TwitchClient;
        public readonly TwitchAPI TwitchApi;
        public Helix Helix => TwitchApi.Helix;
        public Users UsersApi => Helix.Users;
        public Streams StreamsApi => Helix.Streams;

        public User ChannelUser;
        public Stream ChannelStream;

        public readonly List<ServiceBase> Services = new List<ServiceBase>();

        private StreamWriter LogWriter;

        public ArisaTwitchClient()
        {
            TwitchClient = new TwitchClient();

            var credentials = new ConnectionCredentials(Constants.BotUsername, Constants.BotOAuthToken);
            TwitchClient.Initialize(credentials, Constants.ChannelUsername);

            var apiSettings = new ApiSettings
            {
                AccessToken = Constants.BotOAuthToken
            };
            TwitchApi = new TwitchAPI(settings: apiSettings);
        }

        public async Task InitializeAsync()
        {
            LogWriter = new StreamWriter(File.OpenWrite("bot.log"))
            {
                AutoFlush = true
            };

            var usersResponse = await UsersApi.GetUsersAsync(logins: Constants.ChannelUsername.AsList());
            ChannelUser = usersResponse.Users[0];

            ChannelStream = await this.TryGetStreamAsync() ?? throw new Exception("Stream offline!");

            TwitchClient.OnMessageReceived += (_, e) => OnMessageReceived(e.ChatMessage);
            TwitchClient.OnUserJoined += (_, e) => OnUserJoined(e);
            TwitchClient.OnError += (_, e) => Log(e.Exception.ToString());

            TwitchClient.Connect();

            StartServices();
        }
        private void StartServices()
        {
            var hydrationService = new HydrationService(this);
            hydrationService.StartService(Constants.HydrationServiceInterval);

            Services.Add(hydrationService);
        }

        private void OnUserJoined(OnUserJoinedArgs userJoined)
        {
            Log($"{userJoined.Username} joined the channel");
        }

        private void OnMessageReceived(ChatMessage chatMessage)
        {
            if (chatMessage.Message is string text)
            {
                Log($"Chat message from {chatMessage.Username}: {text}");
            }
        }

        public void Log(string message)
        {
            message = $"{DateTime.Now.ToString("HH:mm:ss")} {message}";

            lock (LogWriter)
            {
                Console.WriteLine(message);
                LogWriter.WriteLine(message);
            }
        }
    }
}
