using System;

namespace ArisaTwitchBot.Services
{
    class HydrationService : IntervalService
    {
        public HydrationService(ArisaTwitchClient arisaTwitchClient)
            : base(arisaTwitchClient, nameof(HydrationService))
        { }

        protected override Action IntervalCallback => OnInterval;

        private static readonly string[] _hydrationMessages = new[]
        {
            "Time to take a shot! (of water ofc)",
            "Drink up!",
            "Hydration is important! Chug! Chug! Chug!",
            "Time to water the rice corns."
        };

        private void OnInterval()
        {
            TimeSpan? uptime = ArisaTwitchClient.TryGetStreamUptimeAsync().Result;
            if (uptime.HasValue)
            {
                var uptimeAsDate = new DateTime().AddMinutes(uptime.Value.TotalMinutes);
                Log("stream uptime is " + uptimeAsDate.ToString("HH:mm:ss"));

                int h = (int)uptime.Value.TotalHours;
                int m = uptime.Value.Minutes;

                string hString = $"{h} hour{(h > 1 ? "s" : "")}";
                string mString = $"{m} minute{(m > 1 ? "s" : "")}";

                string message = "You have been up for the past ";

                if (h == 0 && m == 0) message += "few moments";
                else if (h > 0) message += hString + (m > 0 ? (" and " + mString) : "");
                else message += mString;

                message += ". ";
                message += _hydrationMessages.PickRandom();

                SendMessage(message);
            }
            else
            {
                Log("failed to get stream uptime");
                Stop();
            }
        }
    }
}
