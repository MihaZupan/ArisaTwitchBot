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

        protected void SendMessage(string message)
        {
            ArisaTwitchClient.SendMessage(message, ServiceName);
        }

        protected void Log(string message)
        {
            ArisaTwitchClient.Log(ServiceName + ": " + message);
        }
    }
}
