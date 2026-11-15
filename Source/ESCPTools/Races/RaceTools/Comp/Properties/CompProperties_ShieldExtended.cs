using RimWorld;

namespace ESCP_RaceTools
{
    public class CompProperties_ShieldExtended : CompProperties_Shield
    {
        public CompProperties_ShieldExtended() => compClass = typeof(Comp_ShieldExtended);

        public string texPath = "Other/ShieldBubble";
    }
}