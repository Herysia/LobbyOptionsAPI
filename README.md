# LobbyOptionsAPI

Among us tool that allow other mods to easily add custom lobby options

## Features

-   Add custom lobby options (Number, boolean)
-   Options are synced with custom RPC
-   Options are saved in a per-mod config file in among us appdata

## Technical stuff

This mod has been made using [Reactor modding framework](https://github.com/NuclearPowered/Reactor), based on BepInEx, it patches the game at runtime and **DOES NOT** modify any game files.

-   Support Among us v2020.12.9s (Steam only)

### Installation

The DLL should be in `BepInEx/plugins` folder, for developpement you should add assembly reference to it

### Usage example

https://github.com/Herysia/AmongUsTryhard

```cs
using Hazel;
using LobbyOptionsAPI;
using System.IO;

namespace AmongUsTryhard.Patches
{
    public class CustomGameOptionsData : LobbyOptions
    {
        private byte settingsVersion = 1;
        public static CustomGameOptionsData customGameOptions;

        public CustomGameOptionsData() : base(AmongUsTryhard.Id, AmongUsTryhard.rpcSettingsId)
        {
            maxPlayerAdmin = AddOption(10, "Admin max players", 0, 10);
            maxPlayerCams = AddOption(10, "Cams max players", 0, 10);
            maxPlayerVitals = AddOption(10, "Vitals max players", 0, 10);
            hideNames = AddOption(false, "Hide names");
            meetingKillCD = AddOption(10, "Kill cooldown after meeting", 10, 200, 10, "%");
        }

        public CustomNumberOption maxPlayerAdmin;
        public CustomNumberOption maxPlayerCams;
        public CustomNumberOption maxPlayerVitals;
        public CustomToggleOption hideNames;
        public CustomNumberOption meetingKillCD;

        public override void SetRecommendations()
        {
            maxPlayerAdmin.value = 10;
            maxPlayerCams.value = 10;
            maxPlayerVitals.value = 10;
            hideNames.value = false;
            meetingKillCD.value = 100;
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(this.settingsVersion);
            writer.Write((byte)maxPlayerAdmin.value);
            writer.Write((byte)maxPlayerCams.value);
            writer.Write((byte)maxPlayerVitals.value);
            writer.Write(hideNames.value);
            writer.Write((byte)meetingKillCD.value);
        }

        public override void Deserialize(BinaryReader reader)
        {
            try
            {
                SetRecommendations();
                byte b = reader.ReadByte();
                maxPlayerAdmin.value = reader.ReadByte();
                maxPlayerCams.value = reader.ReadByte();
                maxPlayerVitals.value = reader.ReadByte();
                hideNames.value = reader.ReadBoolean();
                meetingKillCD.value = reader.ReadByte();
            }
            catch
            {
            }
        }

        public override void Deserialize(MessageReader reader)
        {
            try
            {
                SetRecommendations();
                byte b = reader.ReadByte();
                maxPlayerAdmin.value = reader.ReadByte();
                maxPlayerCams.value = reader.ReadByte();
                maxPlayerVitals.value = reader.ReadByte();
                hideNames.value = reader.ReadBoolean();
                meetingKillCD.value = reader.ReadByte();
            }
            catch
            {
            }
        }

        public override string ToHudString()
        {
            settings.Length = 0;

            try
            {
                settings.AppendLine();
                settings.AppendLine(
                    $"Max players for: Admin={maxPlayerAdmin.value}, Cams={maxPlayerCams.value}, Vitals={maxPlayerVitals.value}");
                settings.AppendLine($"Hide names: {hideNames.value}");
                settings.Append("Kill cooldown after meeting: ");
                settings.Append(meetingKillCD.value);
                settings.Append("%");
                settings.AppendLine();
            }
            catch
            {
            }

            return settings.ToString();
        }
    }
}
```

# Planned features

-   Handle enum otions

# Contributing

You have encountered a bug or unexpected behaviour ? You want to suggest or add a new feature ? Create an [Issue](https://github.com/Herysia/LobbyOptionsAPI/issues) or [PR](https://github.com/Herysia/LobbyOptionsAPI/pulls) !

### Creating PR

-   [Fork this on github](https://github.com/Herysia/LobbyOptionsAPI/fork)
-   Clone your repo, commit and push your changes
-   Request a new Pull request

# Licensing & Credits

LobbyOptionsAPI is licensed under the MIT License. See [LICENSE](LICENSE.md) for the full License.

Third-party libraries:

-   [Reactor](https://github.com/NuclearPowered/Reactor) is license under the LGPL v3.0 License. See [LICENSE](https://github.com/NuclearPowered/Reactor/blob/master/LICENSE) for the full License.

# Contact

### Discord: Herysia#4293
