﻿using System.Threading.Tasks;

namespace ArisaTwitchBot.Commands
{
    public class SendCommand : ICommand, ICommandAlias
    {
        public string Command => "send";
        public string[] CommandAliases => new[] { "give" };

        public Task Handle(CommandContext context)
        {
            var args = context.Arguments;

            if (args.Count != 2 || args[0].Length < 2 || !args[0].StartsWith('@') || !long.TryParse(args[1], out long amount))
                return context.SendMention($"{context.CommandName.Capitalize()} points by !{context.CommandName} @user amount");

            if (amount < 0 && !(context.IsFromModerator || context.IsSelfCommand))
                return context.SendMention("You can't steal, can you?");

            string receiver = args[0].Substring(1);

            if (amount == 0 || context.User.Username.IgnoreCaseEquals(receiver))
                return context.SendMention($"You've just successfully done nothing");

            if (receiver.IgnoreCaseEquals("all") && context.IsBroadcaster)
            {
                foreach (var user in context.UserService.GetAllUsersUnsafe())
                {
                    user.Balance.ExecuteTransaction(
                        balance => balance.Add(amount));
                }
                return context.SendMention($"Gave everyone {amount}");
            }

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
