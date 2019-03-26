using System;
using System.Threading.Tasks;

namespace ArisaTwitchBot.Commands
{
    public class GambleCommand : ICommand
    {
        public string Command => "gamble";
        private readonly Random _rng = new Random();

        private bool TryOdds(float odds)
        {
            lock (_rng)
            {
                return _rng.NextDouble() < odds;
            }
        }

        public Task Handle(CommandContext context)
        {
            Balance userBalance = context.User.Balance;

            var args = context.Arguments;

            if (args.Count == 0)
                return GambleOne(context, userBalance);

            const string WonZero = "Congratz, you won zero points!";
            string argument = args[0];

            if (args.Count == 1)
            {
                if (argument.IgnoreCaseEquals("all"))
                {
                    return GambleAll(context, userBalance);
                }
                else if (argument.EndsWith('%'))
                {
                    if (int.TryParse(argument.SkipLast(), out int percentage) && percentage >= 0)
                    {
                        return percentage == 0
                            ? context.SendMention(WonZero)
                            : GamblePercentage(context, userBalance, percentage);
                    }
                }
                else if (long.TryParse(argument, out long pointsToGamble))
                {
                    if (pointsToGamble >= 0)
                    {
                        return pointsToGamble == 0
                            ? context.SendMention(WonZero)
                            : GambleValue(context, userBalance, pointsToGamble);
                    }
                }
            }

            return context.SendMention("I don't know how to interpret this");
        }

        private Task GambleOne(CommandContext ctx, Balance userBalance)
        {
            bool won = TryOdds(0.75f);
            long newBalance = -1;

            userBalance.ExecuteTransaction(
                balance =>
                {
                    newBalance = balance.Value + (won ? 1 : -1);
                    balance.Value = newBalance;
                });

            string message = won
                ? $"You won a point to climb all the way up to {newBalance.Abs()}"
                : $"You lost a point to drop down to {newBalance.Abs()}";
            if (newBalance < 0) message += $"point{(newBalance == -1 ? "" : "s")} of debt";

            return ctx.SendMention(message);
        }
        private Task GambleAll(CommandContext ctx, Balance userBalance)
        {
            bool won = TryOdds(0.5f);
            long newBalance = -1;

            if (userBalance.ExecuteTransaction(
                condition: balance => balance > 0,
                ifTrue: balance =>
                {
                    newBalance = won ? balance.Value * 2 : 0;
                    balance.Value = newBalance;
                }))
            {
                if (won) return ctx.SendMention($"You won {Emoji.Ayaya} Your balance has doubled to {newBalance}");
                else return ctx.SendMention($"Odds were not in your favor {Emoji.Sad}");
            }
            else return GambleOne(ctx, userBalance);
        }
        private Task GamblePercentage(CommandContext ctx, Balance userBalance, int percentage)
        {
            if (percentage > 100)
                return ctx.SendMention("That's not how percentages work");

            float fPercentage = percentage / 100f;

            bool won = TryOdds(0.5f);
            long gambled = 0;
            long newBalance = -1;

            if (userBalance.ExecuteTransaction(
                condition: balance => (long)(fPercentage * balance) > 0,
                balance =>
                {
                    gambled = (long)(fPercentage * balance.Value);
                    newBalance = balance.Value + (won ? gambled : -gambled);
                    balance.Value = newBalance;
                }))
            {
                if (won) return ctx.SendMention($"You won {gambled} points and are now at {newBalance} {Emoji.Ayaya}");
                else return ctx.SendMention($"Odds were not in your favor {Emoji.Sad} Losing {gambled} you are now at {newBalance}");
            }
            else return ctx.SendMention("You can't afford to gamble that much");
        }
        private Task GambleValue(CommandContext ctx, Balance userBalance, long value)
        {
            bool won = TryOdds(0.5f);
            long newBalance = -1;

            if (userBalance.ExecuteTransaction(
                condition: balance => balance >= value,
                ifTrue: balance =>
                {
                    newBalance = balance.Value + (won ? value : -value);
                    balance.Value = newBalance;
                }))
            {
                if (won) return ctx.SendMention($"You won {Emoji.Ayaya} Your balance is now {newBalance}");
                else return ctx.SendMention($"Odds were not in your favor {Emoji.Sad} Your balance is now {newBalance}");
            }
            else return ctx.SendMention("You can't afford to gamble that much");
        }
    }
}
