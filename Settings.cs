using Rage;
using RichAmbiance.AmbientEvents;
using System;
using System.Collections.Generic;

namespace RichAmbiance
{
    internal static class Settings
    {
        // Feature Settings
        internal static bool EnableAmbientEvents { get; private set; } = true;

        // Ambient Event settings
        internal static bool DisableEventsWhilePlayerIsBusy { get; private set; } = false;
        internal static Dictionary<EventType, EventFrequency> EventFrequencies { get; } = new Dictionary<EventType, EventFrequency>();
        internal static int EventCooldownTimer { get; private set; } = 5;
        internal static bool EventBlips { get; private set; } = false;
        internal static int CommonEventFrequency { get; private set; } = 70;
        internal static int UncommonEventFrequency { get; private set; } = 20;
        internal static int RareEventFrequency { get; private set; } = CommonEventFrequency + UncommonEventFrequency;
        internal static EventFrequency AssaultFrequency { get; private set; } = EventFrequency.Off;
        internal static EventFrequency CarJackingFrequency { get; private set; } = EventFrequency.Off;
        internal static EventFrequency DrugDealFrequency { get; private set; } = EventFrequency.Off;
        internal static EventFrequency DriveByFrequency { get; private set; } = EventFrequency.Off;
        internal static EventFrequency ProstitutionFrequency { get; private set; } = EventFrequency.Off;
        internal static EventFrequency BOLOFrequency { get; private set; } = EventFrequency.Off;

        // BOLO Settings
        internal static bool EnableBOLOStartBlip { get; private set; } = false;
        internal static int BOLOTimer { get; private set; } = 10;

        private static readonly InitializationFile _ini = new InitializationFile("Plugins/LSPDFR/RichAmbiance.ini");

        internal static void LoadSettings()
        {
            Game.LogTrivial("[Rich Ambiance]: Loading RichAmbiance.ini settings");
            _ini.Create();

            // Feature Settings
            EnableAmbientEvents = _ini.ReadBoolean("Features", "EnableAmbientEvents", false);

            // Ambient Event Settings
            EventCooldownTimer = _ini.ReadInt32("Ambient Events", "EventCooldownTimer", 5);
            EventCooldownTimer *= 60000;
            EventBlips = _ini.ReadBoolean("Ambient Events", "EventBlips", true);
            DisableEventsWhilePlayerIsBusy = _ini.ReadBoolean("Ambient Events", "DisableEventsWhilePlayerIsBusy", false);
            
            CommonEventFrequency = _ini.ReadInt32("Ambient Events", "CommonEventFrequency", 70);
            UncommonEventFrequency = _ini.ReadInt32("Ambient Events", "UnommonEventFrequency", 20);
            ValidateFrequencySettings();
            
            AssaultFrequency = (EventFrequency)Enum.Parse(typeof(EventFrequency), _ini.ReadString("Ambient Events", "AssaultFrequency", "off"), true);
            EventFrequencies.Add(EventType.Assault, AssaultFrequency);
            CarJackingFrequency = (EventFrequency)Enum.Parse(typeof(EventFrequency), _ini.ReadString("Ambient Events", "CarJackingFrequency", "off"), true);
            EventFrequencies.Add(EventType.CarJacking, CarJackingFrequency);
            DrugDealFrequency = (EventFrequency)Enum.Parse(typeof(EventFrequency), _ini.ReadString("Ambient Events", "DrugDealFrequency", "off"), true);
            EventFrequencies.Add(EventType.DrugDeal, DrugDealFrequency);
            DriveByFrequency = (EventFrequency)Enum.Parse(typeof(EventFrequency), _ini.ReadString("Ambient Events", "DriveByFrequency", "off"), true);
            EventFrequencies.Add(EventType.DriveBy, DriveByFrequency);
            ProstitutionFrequency = (EventFrequency)Enum.Parse(typeof(EventFrequency), _ini.ReadString("Ambient Events", "ProstitutionFrequency", "off"), true);
            EventFrequencies.Add(EventType.Prostitution, ProstitutionFrequency);
            BOLOFrequency = (EventFrequency)Enum.Parse(typeof(EventFrequency), _ini.ReadString("Ambient Events", "BOLOFrequency", "off"), true);
            EventFrequencies.Add(EventType.BOLO, BOLOFrequency);

            // BOLO Settings
            EnableBOLOStartBlip = _ini.ReadBoolean("BOLO Settings", "EnableBOLOStartBlip", false);
            BOLOTimer = _ini.ReadInt32("BOLO Settings", "BOLOTimer", 10);
            BOLOTimer *= 60000;
        }

        private static void ValidateFrequencySettings()
        {
            RareEventFrequency = CommonEventFrequency + UncommonEventFrequency;
            if(RareEventFrequency > 100)
            {
                Game.LogTrivial($"[Rich Ambiance]: User's event frequencies are invalid.  Resetting to defaults.");
                CommonEventFrequency = 70;
                UncommonEventFrequency = 20;
                RareEventFrequency = 10;
            }
        }
    }
}
