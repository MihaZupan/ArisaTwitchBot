using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Enums;
using TwitchLib.Api;
using TwitchLib.Api.Core;
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

        public TwitchLib.Api.Helix.Models.Users.User ChannelUser { get; private set; }
        public Stream ChannelStream { get; private set; }

        private readonly Dictionary<Type, ServiceBase> _services;
        public TService GetService<TService>()
            where TService : ServiceBase
        {
            return _services[typeof(TService)] as TService;
        }
        private void AddService<TService>(TService service)
            where TService : ServiceBase
        {
            _services.Add(typeof(TService), service);
        }

        private CommandHandler _commandHandler;
        private UserService _userService;

        private StreamWriter _logWriter;
        private readonly ManualResetEventSlim _joinedToChannel;

        public bool Stopped { get; private set; }
        public void Stop() => Stopped = true;

        public ArisaTwitchClient()
        {
            _joinedToChannel = new ManualResetEventSlim(false);
            _services = new Dictionary<Type, ServiceBase>();
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
            _logWriter = new StreamWriter(File.OpenWrite("bot_" + Environment.TickCount + ".log"))
            {
                AutoFlush = true
            };

            var usersResponse = await UsersApi.GetUsersAsync(logins: Constants.ChannelUsername.AsList());
            ChannelUser = usersResponse.Users[0];

            ChannelStream = await this.TryGetStreamAsync() ?? throw new Exception("Stream offline!");

            TwitchClient.OnUserLeft += (_, e) => OnUserLeft(e.Username);
            TwitchClient.OnMessageReceived += (_, e) => OnMessageReceived(e.ChatMessage);
            TwitchClient.OnJoinedChannel += (_, e) => {
                if (e.Channel.IgnoreCaseEquals(Constants.ChannelUsername))
                    _joinedToChannel.Set();
            };

            TwitchClient.OnNewSubscriber += (_, e) => OnSubscribed(e.Subscriber.UserId, e.Subscriber.SubscriptionPlan);
            TwitchClient.OnReSubscriber += (_, e) => OnSubscribed(e.ReSubscriber.UserId, e.ReSubscriber.SubscriptionPlan);
            TwitchClient.OnGiftedSubscription += (_, e) => OnSubscribed(e.GiftedSubscription.UserId, e.GiftedSubscription.MsgParamSubPlan);
            TwitchClient.OnCommunitySubscription += (_, e) => OnSubscribed(e.GiftedSubscription.UserId, e.GiftedSubscription.MsgParamSubPlan, e.GiftedSubscription.MsgParamMassGiftCount);

            TwitchClient.OnError += (_, e) => Log(e.Exception.ToString());

            StartServices();
            InitializeCommandHandler();

            TwitchClient.Connect();

            SendMessage(
                "[Bot] I'm now online. Feel free to kill me with !stop (or find out what I can do with !commands)",
                nameof(ArisaTwitchClient));
        }

        private void StartServices()
        {
            AddService(new UserService(this));
            AddService(new HydrationService(this));
            AddService(new TwitchPrimeReminderService(this));

            _userService = GetService<UserService>();
        }
        private void InitializeCommandHandler()
        {
            _commandHandler = new CommandHandler(this)
                .Add<StopCommand>()
                .Add<SocialCommand>()
                .Add<BalanceCommand>()
                .Add<GambleCommand>()
                .Add<SendCommand>()
                .Add<PickRandomCommand>()
                .Add<SourceCommand>()
                .Add<TimeCommand>()
                ;

            TwitchClient.OnChatCommandReceived += (_, e) =>
            {
                if (!Stopped)
                    _commandHandler.Handle(e.Command);
            };
        }

        private void OnUserLeft(string username)
        {
            Log($"User {username} left");
            _userService.OnUserLeft(username);
        }
        private void OnMessageReceived(ChatMessage chatMessage)
        {
            _userService.OnUserSpotted(chatMessage.UserId, chatMessage.Username);

            if (chatMessage.Bits > 0)
            {
                RewardUser(chatMessage.UserId, Constants.RewardPerBit * chatMessage.Bits);
            }

            if (chatMessage.Message is string text)
            {
                Log($"Chat message from {chatMessage.Username}: {text}");
            }
        }

        private void OnSubscribed(string senderId, SubscriptionPlan plan, int count = 1)
        {
            long reward = Constants.RewardPerSubscription * count;
            if (plan == SubscriptionPlan.Tier2) reward *= 2;
            if (plan == SubscriptionPlan.Tier3) reward *= 5;
            RewardUser(senderId, reward);
        }

        private void RewardUser(string userId, long reward)
        {
            if (_userService.TryGetUserById(userId, out User user))
            {
                user.Balance.ExecuteTransaction(
                    balance => balance.Add(reward));

                if (reward >= 50)
                {
                    SendMessage(
                        $"@{user.Username} Thank you for supporting the channel! You get {reward} points",
                        sender: nameof(ArisaTwitchClient));
                }
            }
        }


        public void SendMessage(string message, string sender)
        {
            if (Stopped) return;
            if (!_joinedToChannel.IsSet) _joinedToChannel.Wait();
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

            lock (_logWriter)
            {
                Console.WriteLine(message);
                _logWriter.WriteLine(message);
            }
        }
    }
}
