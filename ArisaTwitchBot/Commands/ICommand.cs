using System.Threading.Tasks;

namespace ArisaTwitchBot.Commands
{
    public interface ICommand
    {
        string Command { get; }

        Task Handle(CommandContext context);
    }
}
