using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP.Core
{
    public class RaceExtension_Thoughts : DefModExtension
    {
        public List<ThoughtDef> allowedThoughts = new List<ThoughtDef>();
    }

    [HarmonyPatch(typeof(ThoughtUtility), nameof(ThoughtUtility.CanGetThought))]
    public static class ThoughtUtility_CanGetThought_RaceWhitelist_Patch
    {
        public static void Postfix(ref bool __result, Pawn pawn, ThoughtDef def)
        {
            if (__result && pawn.def.GetModExtension<RaceExtension_Thoughts>() is { } ext && !ext.allowedThoughts.Contains(def))
                __result = false;
        }
    }
}
