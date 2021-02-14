using Discord;

namespace SpeedDatingBot
{
    public static class Overwrites
    {
        public static readonly OverwritePermissions ReadOnly = new OverwritePermissions(
            sendMessages: PermValue.Deny,
            addReactions: PermValue.Deny,
            speak: PermValue.Deny,
            stream: PermValue.Deny
        );

        public static readonly OverwritePermissions ReadWrite = new OverwritePermissions(
            sendMessages: PermValue.Allow,
            addReactions: PermValue.Allow,
            speak: PermValue.Allow,
            stream: PermValue.Allow,
            attachFiles: PermValue.Allow,
            useExternalEmojis: PermValue.Allow,
            mentionEveryone: PermValue.Allow,
            sendTTSMessages: PermValue.Allow,
            readMessageHistory: PermValue.Allow
        );
    }
}