using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Api.Helix.Models.Streams;

namespace ArisaTwitchBot
{
    public static class Helpers
    {
        public static List<string> AsList(this string element)
        {
            return new List<string>(1) { element };
        }

        public static async Task<Stream> TryGetStreamAsync(this ArisaTwitchClient arisaTwitchClient)
        {
            GetStreamsResponse streamsResponse =
                await arisaTwitchClient.StreamsApi.GetStreamsAsync(first: 1, userIds: arisaTwitchClient.ChannelUser.Id.AsList());

            if (streamsResponse.Streams.Length == 1)
                return streamsResponse.Streams[0];

#if DEBUG
            return JsonConvert.DeserializeObject<Stream>(@"
{
	""id"":             ""321321321"",
	""user_id"":        ""123123123"",
	""user_name"":      ""MOCKUSER"",
	""game_id"":        ""12345"",
	""type"":           ""live"",
	""title"":          ""MOCK TITLE"",
	""viewer_count"":   9001,
	""started_at"":     ""2019-03-22T16:00:00Z"",
	""language"":       ""en""
}
");
#else
            return null;
#endif
        }

        public static async Task<TimeSpan?> TryGetStreamUptimeAsync(this ArisaTwitchClient arisaTwitchClient)
        {
            Stream stream = await TryGetStreamAsync(arisaTwitchClient);

            if (stream == null)
                return null;

            return DateTime.UtcNow.Subtract(stream.StartedAt);
        }

        public static T PickRandom<T>(this T[] array)
            => array[new Random().Next(array.Length)];

        public static T PickRandom<T>(this Random rng, T[] array)
            => array[rng.Next(array.Length)];

        public static bool IgnoreCaseEquals(this string left, string right)
            => left.Equals(right, StringComparison.OrdinalIgnoreCase);

        public static ReadOnlySpan<char> SkipLast(this string source)
            => source.AsSpan(0, source.Length - 1);

        public static long Abs(this long value)
            => Math.Abs(value);

        public static string Capitalize(this string source)
        {
            return char.ToUpper(source[0]) + source.Substring(1);
        }
    }
}
