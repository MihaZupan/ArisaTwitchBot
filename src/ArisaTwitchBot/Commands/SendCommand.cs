using System.Threading.Tasks;

namespace ArisaTwitchBot.Commands
{
    public class SendCommand : ICommand
    {
        public string Command => "send";

        public Task Handle(CommandContext context)
        {
            var args = context.Arguments;

            if (args.Count != 2 || args[0].Length < 2 || !args[0].StartsWith('@') || !long.TryParse(args[1], out long amount))
                return context.SendMention("Send points by !send @user amount");

            if (amount < 0 && !context.IsFromModerator)
                return context.SendMention("You can't steal, can you?");

            string receiver = args[0].Substring(1);

            if (amount == 0 || context.User.Username.IgnoreCaseEquals(receiver))
                return context.SendMention($"You've just successfully done nothing");

            if (!context.UserService.TryGetUserByUsername(receiver, out User receivingUser))
                return context.SendMention(
                    "I do not know anyone by that name. @" + receiver +
                    " perhaps you should say something in chat.");

            Balance senderBalance = context.User.Balance;
            Balance receiverBalance = receivingUser.Balance;

            if (senderBalance.ExecuteTransaction(
                condition: balance => balance >= amount,
                ifTrue: balance => balance.Substract(amount)))
            {
                receiverBalance.ExecuteTransaction(
                    balance => balance.Add(amount));

                if (amount > 0) return context.SendMention($"Successfully sent {amount} to @{receiver}");
                else return context.SendMention($"Successfully stole {-amount} from @{receiver}");
            }
            else
            {
                return context.SendMention("You do not have that many points to send");
            }
        }
    }
}
