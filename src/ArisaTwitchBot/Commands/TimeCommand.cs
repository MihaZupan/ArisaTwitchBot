using NodaTime;
using System.Globalization;
using System.Threading.Tasks;

namespace ArisaTwitchBot.Commands
{
    public class TimeCommand : ICommand
    {
        public string Command => "time";

        private static readonly ZonedClock _clock = new ZonedClock(
            SystemClock.Instance, DateTimeZoneProviders.Tzdb[Constants.BroadcasterTimeZone], CalendarSystem.Gregorian);

        public Task Handle(CommandContext context)
        {
            var localTime = _clock.GetCurrentTimeOfDay();
            string localTimeString = localTime.ToString("h:mm tt", CultureInfo.InvariantCulture);
            return context.SendMessage("Broadcaster's local time is " + localTimeString);
        }
    }
}
