using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ArisaTwitchBot
{
    public class Program
    {
        static ArisaTwitchClient ArisaBot;

        static async Task Main()
        {
            if (!File.Exists("bot.db"))
                File.Copy("../../../bot.db", "bot.db");

            ArisaBot = new ArisaTwitchClient();
            await ArisaBot.InitializeAsync();

            await Task.Delay(Timeout.Infinite);
        }
    }
}
