using ArisaTwitchBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace ArisaTwitchBot.Commands
{
    public class CommandContext
    {
        public readonly ArisaTwitchClient ArisaTwitchClient;
        public readonly ChatCommand ChatCommand;
        public readonly string CommandName;

        public ChatMessage ChatMessage => ChatCommand.ChatMessage;
        public bool IsBroadcaster => ChatMessage.IsBroadcaster;
        public bool IsFromModerator => ChatMessage.IsModerator || IsBroadcaster;
        public List<string> Arguments => ChatCommand.ArgumentsAsList;

        public readonly UserService UserService;
        public readonly User User;
        public readonly bool IsSelfCommand;

        public CommandContext(ArisaTwitchClient arisaTwitchClient, ChatCommand chatCommand)
        {
            ArisaTwitchClient = arisaTwitchClient;
            ChatCommand = chatCommand;
            CommandName = chatCommand.CommandText;

            UserService = GetService<UserService>();
            UserService.TryGetUserById(ChatMessage.UserId, out User);
            IsSelfCommand = User.Username.IgnoreCaseEquals(Constants.BotUsername);
        }

        public async Task SendMessage(string message)
        {
            if (IsSelfCommand)
                await Task.Delay(Constants.OnSelfReplyDelay);

            ArisaTwitchClient.SendMessage(message, CommandName);
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
