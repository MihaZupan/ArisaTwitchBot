using System.Threading;
using System.Threading.Tasks;

namespace ArisaTwitchBot
{
    class Program
    {
        static ArisaTwitchClient ArisaBot;

        static async Task Main()
        {
            ArisaBot = new ArisaTwitchClient();
            await ArisaBot.InitializeAsync();

            await Task.Delay(Timeout.Infinite);
        }
    }
}
