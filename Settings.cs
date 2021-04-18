using Rage;
using System.Collections.Generic;

namespace RichAmbiance
{
    internal static class Settings
    {
        // Feature Settings
        internal static bool EnableBOLO { get; private set; } = false;
        internal static bool EnableAmbientEvents { get; private set; } = false;

        // Ambient Event settings
        internal static Dictionary<string, string> EventFrequencies { get; } = new Dictionary<string, string>();
        internal static int EventCooldownTimer { get; private set; } = 5;
        internal static bool EventBlips { get; private set; } = false;
        internal static int CommonEventFrequency { get; private set; } = 70;
        internal static int UncommonEventFrequency { get; private set; } = 20;
        internal static int RareEventFrequency { get; private set; } = 10;
        internal static string AssaultFrequency { get; private set; } = "off";
        internal static string CarJackingFrequency { get; private set; } = "off";
        internal static string DrugDealFrequency { get; private set; } = "off";
        internal static string DriveByFrequency { get; private set; } = "off";
        internal static string ProstitutionFrequency { get; private set; } = "off";

        // BOLO Settings
        internal static bool EnableBOLOStartBlip { get; private set; } = false;
        internal static int BOLOTimer { get; private set; } = 10;
        internal static int BOLOFrequency { get; private set; } = 5;

        private static readonly InitializationFile _ini = new InitializationFile("Plugins/LSPDFR/RichAmbiance.ini");

        internal static void LoadSettings()
        {
            Game.LogTrivial("[RPE]: Loading RichAmbiance.ini settings");
            _ini.Create();

            // Feature Settings
            EnableBOLO = _ini.ReadBoolean("Features", "EnableBOLO", false);
            EnableAmbientEvents = _ini.ReadBoolean("Features", "EnableAmbientEvents", false);

            // Ambient Event Settings
            EventCooldownTimer = _ini.ReadInt32("Ambient Events", "EventCooldownTimer", 5);
            EventCooldownTimer *= 60000;
            EventBlips = _ini.ReadBoolean("Ambient Events", "EventBlips", true);
            CommonEventFrequency = _ini.ReadInt32("Ambient Events", "CommonEventFrequency", 70);
            UncommonEventFrequency = _ini.ReadInt32("Ambient Events", "UnommonEventFrequency", 20);
            RareEventFrequency = _ini.ReadInt32("Ambient Events", "RareEventFrequency", 10);
            AssaultFrequency = _ini.ReadString("Ambient Events", "AssaultFrequency", "off");
            EventFrequencies.Add("Assault", AssaultFrequency);
            CarJackingFrequency = _ini.ReadString("Ambient Events", "CarJackingFrequency", "off");
            EventFrequencies.Add("CarJacking", CarJackingFrequency);
            DrugDealFrequency = _ini.ReadString("Ambient Events", "DrugDealFrequency", "off");
            EventFrequencies.Add("DrugDeal", DrugDealFrequency);
            DriveByFrequency = _ini.ReadString("Ambient Events", "DriveByFrequency", "off");
            EventFrequencies.Add("DriveBy", DriveByFrequency);
            ProstitutionFrequency = _ini.ReadString("Ambient Events", "ProstitutionFrequency", "off");
            EventFrequencies.Add("Prostitution", ProstitutionFrequency);

            // BOLO Settings
            EnableBOLOStartBlip = _ini.ReadBoolean("BOLO Settings", "EnableBOLOStartBlip", false);
            BOLOTimer = _ini.ReadInt32("BOLO Settings", "BOLOTimer", 10);
            BOLOTimer *= 60000;
            BOLOFrequency = _ini.ReadInt32("BOLO Settings", "BOLOFrequency", 5);
            BOLOFrequency *= 60000;
        }
    }
}
