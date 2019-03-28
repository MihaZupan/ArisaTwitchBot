using System.Threading.Tasks;

namespace ArisaTwitchBot.Commands
{
    public class SourceCommand : ICommand
    {
        public string Command => "source";

        public Task Handle(CommandContext context)
        {
            return context.SendMention($"Find out how I function at {Constants.SourceRepositoryUrl}");
        }
    }
}
