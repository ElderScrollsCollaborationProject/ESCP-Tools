using Verse;
using RimWorld;

namespace ESCP_Skyshards
{
    [DefOf]
    public static class ThingDefOf
    {
        static ThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
        }

        [MayRequire("Rick.ESCP.Skyshards")]

        public static ThingDef ESCP_SkyshardBuilding;
        [MayRequire("Rick.ESCP.Skyshards")]
        public static ThingDef ESCP_SkyshardSculpture;
    }
}
