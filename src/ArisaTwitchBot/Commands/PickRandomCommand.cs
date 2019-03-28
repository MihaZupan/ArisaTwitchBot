using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArisaTwitchBot.Commands
{
    public class PickRandomCommand : ICommand
    {
        public string Command => "pickrandom";

        public Task Handle(CommandContext context)
        {
            int count = 1;
            var args = context.Arguments;
            if (args.Count == 1 && int.TryParse(args[0], out int newCount) && newCount > 0)
                count = newCount;

            var allUsers = context.UserService.GetAllActiveUsersUnsafe();

            count = Math.Min(count, allUsers.Length - (context.IsBroadcaster ? 1 : 2));
            count = Math.Min(count, 5);

            Random rng = new Random();
            string[] randomUsernames = new string[count];

            for (int i = 0; i < randomUsernames.Length; i++)
            {
                while (true)
                {
                    var randUser = rng.PickRandom(allUsers).Key;

                    if (randomUsernames.Contains(randUser, StringComparer.OrdinalIgnoreCase) ||
                        randUser.IgnoreCaseEquals(Constants.ChannelUsername) ||
                        (!context.IsBroadcaster && randUser.IgnoreCaseEquals(context.User.Username)))
                        continue;

                    randomUsernames[i] = randUser;
                    break;
                }
            }

            if (count == 0)
                return context.SendMention("It's lonely in here");

            if (count == 1)
                return context.SendMessage($"Here's a random person for ya: @{randomUsernames[0]}");

            return context.SendMessage(
                "Here's a few random people for ya: " +
                string.Join(' ', randomUsernames.Select(user => "@" + user)));
        }
    }
}
