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

        public void StartService(TimeSpan interval)
        {
            if (Enabled) throw new InvalidOperationException("Service already started");
            Enabled = true;

            Log("starting");
            _intervalTimer = new Timer(_ => OnInterval(), null, dueTime: 1, period: (int)interval.TotalMilliseconds);
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
