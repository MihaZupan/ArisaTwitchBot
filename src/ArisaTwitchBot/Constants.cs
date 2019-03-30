using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace ArisaTwitchBot
{
    public static class Constants
    {
        public const string BotUsername = "MihaZupan";
        public static readonly string OAuthToken;
        public static readonly string OAuthRefreshToken;

        static Constants()
        {
            const string secretsPath = "../../secrets.json";
            if (!File.Exists(secretsPath)) throw new Exception("No secrets found -.-");

            JObject secrets = JObject.Parse(File.ReadAllText(secretsPath));

            OAuthToken = secrets["token"].ToObject<string>();
            OAuthRefreshToken = secrets["refreshToken"].ToObject<string>();
        }

#if DEBUG
        public const string ChannelUsername = "MihaZupan";
#else
        public const string ChannelUsername = "xArisax";
#endif

        public const long InitialUserBalance = 100;
        public const long RewardPerSubscription = 500;
        public const long RewardPerBit = 2;

        public const string SourceRepositoryUrl = "https://github.com/MihaZupan/ArisaTwitchBot";

        public const string BroadcasterTimeZone = "Europe/Berlin";

        public const int OnSelfReplyDelay = 3000;
    }
}
