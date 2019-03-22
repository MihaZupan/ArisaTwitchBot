namespace ArisaTwitchBot.Services
{
    public abstract class ServiceBase
    {
        protected readonly ArisaTwitchClient ArisaTwitchClient;

        protected readonly string ServiceName;

        protected ServiceBase(ArisaTwitchClient arisaTwitchClient, string serviceName)
        {
            ArisaTwitchClient = arisaTwitchClient;
            ServiceName = serviceName;
        }

        public abstract void Stop();

        protected void SendMessage(string message)
        {
            ArisaTwitchClient.TwitchClient.SendMessage(ArisaTwitchClient.ChannelUsername, message);
            Log($"sent a message \"{message}\"");
        }

        protected void Log(string message)
        {
            ArisaTwitchClient.Log(ServiceName + ": " + message);
        }
    }
}
