using System;

namespace ArisaTwitchBot
{
    public struct PeriodAndOffset
    {
        public TimeSpan Period;
        public TimeSpan Offset;

        public PeriodAndOffset(TimeSpan period, TimeSpan offset)
        {
            Period = period;
            Offset = offset;
        }

        public static PeriodAndOffset FromMinutes(int periodMinutes, int offsetMinutes)
        {
            return new PeriodAndOffset(TimeSpan.FromMinutes(periodMinutes), TimeSpan.FromMinutes(offsetMinutes));
        }
    }
}
