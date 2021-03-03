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

        public static readonly OverwritePermissions BotPermissions = new OverwritePermissions(
            moveMembers: PermValue.Allow,
            viewChannel: PermValue.Allow,
            manageChannel: PermValue.Allow,
            manageRoles: PermValue.Allow
        );

        public static readonly OverwritePermissions FullDeny = new OverwritePermissions(
            sendMessages: PermValue.Deny,
            connect: PermValue.Deny,
            viewChannel: PermValue.Deny
        );

        public static readonly OverwritePermissions ConnectVoice = new OverwritePermissions(
            connect: PermValue.Allow,
            speak: PermValue.Allow,
            useVoiceActivation: PermValue.Allow
        );
    }
}