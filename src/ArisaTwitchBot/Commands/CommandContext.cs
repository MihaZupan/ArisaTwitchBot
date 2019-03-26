using ArisaTwitchBot.Services;
using ArisaTwitchBot.Services.Database;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace ArisaTwitchBot.Commands
{
    public class CommandContext
    {
        public readonly ArisaTwitchClient ArisaTwitchClient;
        public readonly ChatCommand ChatCommand;
        private readonly string _commandName;

        public ChatMessage ChatMessage => ChatCommand.ChatMessage;
        public bool IsBroadcaster => ChatMessage.IsBroadcaster;
        public bool IsFromModerator => ChatMessage.IsModerator || IsBroadcaster || ChatMessage.IsMe;
        public List<string> Arguments => ChatCommand.ArgumentsAsList;

        public readonly UserService UserService;
        public readonly User User;

        public CommandContext(ArisaTwitchClient arisaTwitchClient, ChatCommand chatCommand, string commandName)
        {
            ArisaTwitchClient = arisaTwitchClient;
            ChatCommand = chatCommand;
            _commandName = commandName;

            UserService = GetService<UserService>();
            UserService.TryGetUserById(ChatMessage.UserId, out User);
        }

        public Task SendMessage(string message)
        {
            ArisaTwitchClient.SendMessage(message, _commandName);
            return Task.CompletedTask;
        }
        public Task SendMention(string message)
        {
            return SendMessage($"@{User.Username} {message}");
        }

        public TService GetService<TService>()
            where TService : ServiceBase
        {
            return ArisaTwitchClient.GetService<TService>();
        }
    }
}
