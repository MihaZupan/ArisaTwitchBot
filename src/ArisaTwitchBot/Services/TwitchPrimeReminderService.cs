using System;

namespace ArisaTwitchBot.Services
{
    public class TwitchPrimeReminderService : IntervalService
    {
        public TwitchPrimeReminderService(ArisaTwitchClient arisaTwitchClient)
            : base(arisaTwitchClient, nameof(TwitchPrimeReminderService))
        { }

        protected override Action IntervalCallback => OnInterval;

        protected override PeriodAndOffset PeriodAndOffset => PeriodAndOffset.FromMinutes(60, 35);

        private void OnInterval()
        {
            SendMessage("You can subscribe for free using Twitch Prime!");
        }
    }
}
