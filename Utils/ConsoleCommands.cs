using Rage.Attributes;
using Rage.ConsoleCommands.AutoCompleters;
using Rage;
using RichAmbiance.AmbientEvents;
using System.Reflection;

namespace RichAmbiance.Utils
{
    using RichAmbiance.Features;
    
    [Obfuscation(Exclude = false, Feature = "-rename", ApplyToMembers = false)]
    internal static class ConsoleCommands
    {
        [ConsoleCommand("BeginAmbientEvent")]
        internal static void Command_BeginAmbientEvent([ConsoleCommandParameter(AutoCompleterType = typeof(ConsoleCommandParameterAutoCompleterEnum), Name = "BeginAmbientEvent")] EventType eventType)
        {
            AmbientEvents.InitializeNewEvent(eventType);
        }

        [ConsoleCommand("EndAmbientEvent")]
        internal static void Command_EndAmbientEvent([ConsoleCommandParameter(AutoCompleterType = typeof(ConsoleCommandAutoCompleterBoolean), Name = "EndAmbientEvent")] bool end = true)
        {
            if (AmbientEvents.ActiveEvent == null)
            {
                Game.LogTrivial($"ActiveEvent is null.");
                return;
            }

            AmbientEvents.ActiveEvent.TransitionToState(State.Ending);
        }
    }
}
