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
            if (context.IsFromModerator)
            {
                context.SendMention("Goodbye!");
                context.ArisaTwitchClient.Stop();
                Thread.Sleep(1000); // Wait for the message to go through
                Environment.Exit(0);
                return null; // We're not getting to here
            }
            else
            {
                return context.SendMention("You have no power over me!");
            }
        }
    }
}
