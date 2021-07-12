namespace DudesBot
{
    public class BotSettingsObject
    {
        public string Token { get; init; }
        public ulong PinChannel { get; init; }
        public ulong[] PinWatchedList { get; init; }
        public bool BotWatch { get; init; }
        public string[] CommandPrefix { get; init; }
        public string DBPath {get;init;}
    }
}