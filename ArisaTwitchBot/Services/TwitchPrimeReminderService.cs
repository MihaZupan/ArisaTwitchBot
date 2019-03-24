using System;

namespace ArisaTwitchBot.Services
{
    public class TwitchPrimeReminderService : IntervalService
    {
        public TwitchPrimeReminderService(ArisaTwitchClient arisaTwitchClient)
            : base(arisaTwitchClient, nameof(TwitchPrimeReminderService))
        { }

        protected override Action IntervalCallback => OnInterval;

        private void OnInterval()
        {
            SendMessage("You can subscribe for free using Twitch Prime!");
        }
    }
}
