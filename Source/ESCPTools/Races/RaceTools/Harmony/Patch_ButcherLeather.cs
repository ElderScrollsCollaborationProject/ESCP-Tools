using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace ESCP_RaceTools
{
    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.LatePatchesCategory)]
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.ButcherProducts))]
    public static class Pawn_ButcherProducts_Leather_Patch
    {
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, Pawn __instance)
        {
            ThingDef leather = RaceProperties.For(__instance)?.leatherDef;
            foreach (Thing thing in __result)
            {
                if (leather == null || thing.def != __instance.RaceProps.leatherDef)
                {
                    yield return thing;
                    continue;
                }
                Thing replaced = ThingMaker.MakeThing(leather);
                replaced.stackCount = thing.stackCount;
                yield return replaced;
            }
        }
    }
}
