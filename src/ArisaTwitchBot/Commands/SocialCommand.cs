using System.Threading.Tasks;

namespace ArisaTwitchBot.Commands
{
    public class SocialCommand : ICommand
    {
        public string Command => "social";

        public Task Handle(CommandContext context)
        {
            const string message = "Make sure to follow instagram.com/asian_arisa";

            return context.SendMessage(message);
        }
    }
}
