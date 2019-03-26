using System;
using System.Threading.Tasks;

namespace ArisaTwitchBot.Commands
{
    public class BalanceCommand : ICommand
    {
        public string Command => "balance";

        public Task Handle(CommandContext context)
        {
            long currentBalance = context.User.Balance.ReadUnsafe();

            bool inDebt = currentBalance < 0;
            currentBalance = Math.Abs(currentBalance);

            string pointsString = currentBalance + " point" + (currentBalance == 1 ? "" : "s");

            if (inDebt)
            {
                return context.SendMention($"You are {pointsString} in debt");
            }
            else
            {
                return context.SendMention($"You have {pointsString}");
            }
        }
    }
}
