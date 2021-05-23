using Rage;
using System.Collections.Generic;
using System.Linq;

namespace RichAmbiance.Utils
{
    internal static class HelperMethods
    {
        internal static List<Ped> GetReleventPedsForAmbientEvent() => World.GetAllPeds().Where(p => p.IsRelevantForAmbientEvent() && p.IsAmbient()).ToList();
    }
}
