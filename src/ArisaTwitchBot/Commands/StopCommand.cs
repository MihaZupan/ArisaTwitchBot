using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArisaTwitchBot.Commands
{
    public class StopCommand : ICommand
    {
        public string Command => "stop";

        public Task Handle(CommandContext context)
        {
            if (context.IsFromModerator || context.Sender.Equals(Constants.BotUsername, StringComparison.OrdinalIgnoreCase))
            {
                context.SendMention("Goodbye!");
                Thread.Sleep(1000);
                Environment.Exit(0);
            }
            else
            {
                context.SendMention("You have no power over me!");
            }
            return Task.CompletedTask;
        }
    }
}
