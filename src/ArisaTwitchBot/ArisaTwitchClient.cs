using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
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
using ArisaTwitchBot.Commands;

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
        public CommandHandler CommandHandler;

        private StreamWriter LogWriter;
        private readonly ManualResetEventSlim JoinedToChannel = new ManualResetEventSlim(false); 

        public ArisaTwitchClient()
        {
            TwitchClient = new TwitchClient();

            var credentials = new ConnectionCredentials(Constants.BotUsername, Constants.OAuthToken);
            TwitchClient.Initialize(credentials, Constants.ChannelUsername);

            var apiSettings = new ApiSettings
            {
                AccessToken = Constants.OAuthToken
            };
            TwitchApi = new TwitchAPI(settings: apiSettings);
        }

        public async Task InitializeAsync()
        {
            LogWriter = new StreamWriter(File.OpenWrite("bot_" + Environment.TickCount + ".log"))
            {
                AutoFlush = true
            };

            var usersResponse = await UsersApi.GetUsersAsync(logins: Constants.ChannelUsername.AsList());
            ChannelUser = usersResponse.Users[0];

            ChannelStream = await this.TryGetStreamAsync() ?? throw new Exception("Stream offline!");

            TwitchClient.OnUserJoined += (_, e) => OnUserJoined(e);
            TwitchClient.OnMessageReceived += (_, e) => OnMessageReceived(e.ChatMessage);
            TwitchClient.OnJoinedChannel += (_, e) => {
                if (e.Channel.Equals(Constants.ChannelUsername, StringComparison.OrdinalIgnoreCase))
                    JoinedToChannel.Set();
            };

            TwitchClient.OnError += (_, e) => Log(e.Exception.ToString());

            InitializeCommandHandler();

            TwitchClient.Connect();

            StartServices();

            Log(nameof(ArisaTwitchClient) + " initialized");

            SendMessage(
                "[Bot] I'm now online. Feel free to kill me with !stop (or find out what I can do with !commands)",
                nameof(ArisaTwitchClient));
        }

        private void StartServices()
        {
            Services.AddRange(new[]
            {
                new HydrationService(this).Start(Constants.HydrationServicePeriodAndOffset),
                new TwitchPrimeReminderService(this).Start(Constants.TwitchPrimeReminderPeriodAndOffset)
            });
        }
        private void InitializeCommandHandler()
        {
            CommandHandler = new CommandHandler(this)
                .Add<SocialCommand>()
                .Add<StopCommand>()
                ;

            TwitchClient.OnChatCommandReceived += (_, e) => CommandHandler.Handle(e.Command);
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

        public void SendMessage(string message, string sender)
        {
            if (!JoinedToChannel.IsSet) JoinedToChannel.Wait();
            TwitchClient.SendMessage(Constants.ChannelUsername, message);
            Log($"{sender} sent a message \"{message}\"");
        }

        public void LogException(Exception exception)
        {
            Log(Environment.StackTrace + '\n' + exception.ToString());
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
