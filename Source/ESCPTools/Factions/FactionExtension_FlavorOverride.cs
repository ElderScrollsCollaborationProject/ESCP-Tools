using RimWorld;
using Verse;

namespace ESCP.Factions
{
    public class FactionExtension_FlavorOverride : DefModExtension
    {
        public string newLabel;
        public bool preferFactionLeaderTitle = false;

        public static string TryGetLabel(FactionDef factionDef)
        {
            var ext = factionDef.GetModExtension<FactionExtension_FlavorOverride>();
            return ext?.newLabel;
        }
    }
}
