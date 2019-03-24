using TwitchLib.Client.Models;

namespace ArisaTwitchBot.Commands
{
    public class CommandContext
    {
        public readonly ArisaTwitchClient ArisaTwitchClient;
        public readonly ChatCommand ChatCommand;
        private readonly string _commandName;

        public ChatMessage ChatMessage => ChatCommand.ChatMessage;
        public string Sender => ChatMessage.Username;
        public bool IsBroadcaster => ChatMessage.IsBroadcaster;
        public bool IsFromModerator => ChatMessage.IsModerator || IsBroadcaster;

        public CommandContext(ArisaTwitchClient arisaTwitchClient, ChatCommand chatCommand, string commandName)
        {
            ArisaTwitchClient = arisaTwitchClient;
            ChatCommand = chatCommand;
            _commandName = commandName;
        }

        public void SendMessage(string message)
        {
            ArisaTwitchClient.SendMessage(message, _commandName);
        }
        public void SendMention(string message)
        {
            SendMessage($"@{Sender} {message}");
        }
    }
}
