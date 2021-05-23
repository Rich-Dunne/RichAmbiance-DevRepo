using LSPD_First_Response.Mod.API;
using Rage;
using System.Collections.Generic;
using System.Linq;
using RichAmbiance.AmbientEvents;

namespace RichAmbiance.Utils
{
    internal static class GuardClauses
    {
        internal static bool EventPedsFound(IEnumerable<EventPed> eventPeds, int numberOfPedsNeeded, int attempt)
        {
            if (eventPeds.Count() == numberOfPedsNeeded)
            {
                Game.LogTrivial($"[Rich Ambiance]: Success on attempt {attempt}");
                return true;
            }
            return false;
        }
    }
}
