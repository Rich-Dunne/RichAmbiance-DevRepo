using LSPD_First_Response.Mod.API;
using Rage;
using System.Collections.Generic;
using System.Linq;
using RichAmbiance.AmbientEvents;

namespace RichAmbiance.Utils
{
    internal static class GuardClauses
    {
        internal static bool CalloutOrPursuitActive()
        {
            if (Functions.IsCalloutRunning() || Functions.GetActivePursuit() != null)
            {
                Game.LogTrivial($"[RPE Ambient Event]: Callout or pursuit is running.  Ending event.");
                return true;
            }
            return false;
        }

        internal static bool EventPedsFound(IEnumerable<EventPed> eventPeds, int numberOfPedsNeeded, int attempt)
        {
            if (eventPeds.Count() == numberOfPedsNeeded)
            {
                Game.LogTrivial($"[RPE Ambient Event]: Success on attempt {attempt}");
                return true;
            }
            return false;
        }
    }
}
