namespace DudesBot
{
    public class BotSettingsObject
    {
        public string Token { get; init; }
        public ulong PinChannel { get; init; }
        public ulong[] PinWatchedList { get; init; }
        public bool BotWatch { get; init; }
        public string[] CommandPrefix { get; init; }
        public string DBPath { get; init; }
        public ulong[] QuietChannels {get; init;}
        public bool WarningCommand {get; init;}
        public bool ReminderCommand {get; init;}
        public bool UndeleteCommand {get; init;}
        public ulong GuildID {get; init;}
    }
}