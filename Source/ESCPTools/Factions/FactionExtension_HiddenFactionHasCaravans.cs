using RimWorld;
using Verse;

namespace ESCP.Factions
{
    public class FactionExtension_HiddenFactionHasCaravans : DefModExtension
    {
        public static bool FactionHas(Faction faction)
        {
            return faction.def.HasModExtension<FactionExtension_HiddenFactionHasCaravans>();
        }
    }
}
