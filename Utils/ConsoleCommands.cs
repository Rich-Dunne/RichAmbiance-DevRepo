using Rage.Attributes;
using Rage.ConsoleCommands.AutoCompleters;
using Rage;
using RichAmbiance.AmbientEvents;
using System.Reflection;

namespace RichAmbiance.Utils
{
    using RichAmbiance.Features;
    using System.Linq;

    [Obfuscation(Exclude = false, Feature = "-rename", ApplyToMembers = false)]
    internal static class ConsoleCommands
    {
        [ConsoleCommand("BeginAmbientEvent")]
        internal static void Command_BeginAmbientEvent([ConsoleCommandParameter(AutoCompleterType = typeof(ConsoleCommandParameterAutoCompleterEnum), Name = "BeginAmbientEvent")] EventType eventType)
        {
            AmbientEvents.InitializeNewEvent(eventType);
        }

        [ConsoleCommand("EndAmbientEvent")]
        internal static void Command_EndAmbientEvent([ConsoleCommandParameter(AutoCompleterType = typeof(ConsoleCommandParameterAutoCompleterEnum), Name = "EndAmbientEvent")] EventType eventType)
        {
            if (AmbientEvents.ActiveEvents.Count == 0)
            {
                Game.LogTrivial($"There are no active events.");
                return;
            }

            if (!AmbientEvents.ActiveEvents.Any(x => x.EventType == eventType))
            {
                Game.LogTrivial($"There are no active {eventType} events.");
                return;
            }

            AmbientEvents.ActiveEvents.Where(x => x.EventType == eventType).ToList().ForEach(x => x.TransitionToState(State.Ending));
        }

        [ConsoleCommand("EndAllAmbientEvents")]
        internal static void Command_EndAllAmbientEvents()
        {
            Game.LogTrivial($"[Rich Ambiance]: Ending {AmbientEvents.ActiveEvents.Count()} active events.");
            AmbientEvents.ActiveEvents.ForEach(x => x.TransitionToState(State.Ending));
            Game.LogTrivial($"[Rich Ambiance]: All events terminated.");
        }

        [ConsoleCommand("GetActiveAmbientEvents")]
        internal static void Command_GetActiveAmbientEvents()
        {
            if(AmbientEvents.ActiveEvents.Count == 0)
            {
                Game.LogTrivial($"[Rich Ambiance]: There are no events active.");
                return;
            }

            AmbientEvents.ActiveEvents.ForEach(x =>
            {
                Game.LogTrivial($"[Rich Ambiance]: Current active event: {x.EventType}");
            });
        }
    }
}
