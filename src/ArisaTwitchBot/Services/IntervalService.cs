using System;
using System.Threading;

namespace ArisaTwitchBot.Services
{
    public abstract class IntervalService : ServiceBase
    {
        protected abstract Action IntervalCallback { get; }
        protected abstract PeriodAndOffset PeriodAndOffset { get; }
        private Timer _intervalTimer;

        protected IntervalService(ArisaTwitchClient arisaTwitchClient, string serviceName)
            : base(arisaTwitchClient, serviceName)
        {
            Log($"starting in {PeriodAndOffset.Offset} ms");
            _intervalTimer = new Timer(_ => IntervalCallback(),
                state: null,
                dueTime: PeriodAndOffset.Offset,
                period: PeriodAndOffset.Period);
        }
    }
}
