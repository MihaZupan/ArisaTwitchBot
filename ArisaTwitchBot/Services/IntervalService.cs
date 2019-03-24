using System;
using System.Threading;

namespace ArisaTwitchBot.Services
{
    public abstract class IntervalService : ServiceBase
    {
        public bool Enabled { get; private set; }

        protected abstract Action IntervalCallback { get; }
        private Timer _intervalTimer;

        protected IntervalService(ArisaTwitchClient arisaTwitchClient, string serviceName)
            : base(arisaTwitchClient, serviceName)
        { }

        public IntervalService Start(PeriodAndOffset periodAndOffset)
        {
            if (Enabled) throw new InvalidOperationException("Service already started");
            Enabled = true;

            Log($"starting in {periodAndOffset.Offset} ms");
            _intervalTimer = new Timer(_ => OnInterval(),
                state: null,
                dueTime: periodAndOffset.Offset,
                period: periodAndOffset.Period);

            return this;
        }

        private void OnInterval()
        {
            if (Enabled)
            {
                IntervalCallback();
            }
        }

        public override void Stop()
        {
            if (!Enabled) return;
            Enabled = false;

            _intervalTimer.Dispose();
            Log("service stopped");
        }
    }
}
