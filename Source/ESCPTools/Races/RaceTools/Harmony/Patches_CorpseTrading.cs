using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP_RaceTools
{

    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.LatePatchesCategory)]
    [HarmonyPatch(typeof(TradeUtility))]
    [HarmonyPatch("EverPlayerSellable")]
    public static class TradeUtility_EverPlayerSellable_CorpsePatch
    {
        public static void Postfix(ThingDef def, ref bool __result)
        {
            if (!__result)
            {
                if (def.IsCorpse)
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.LatePatchesCategory)]
    [HarmonyPatch(typeof(StockGenerator_BuyExpensiveSimple))]
    [HarmonyPatch("HandlesThingDef")]
    public static class StockGenerator_BuyExpensiveSimple_CorpsePatch
    {
        public static void Postfix(ThingDef thingDef, ref bool __result)
        {
            if (__result)
            {
                if (thingDef.IsCorpse)
                {
                    __result = false;
                }
            }
        }
    }
}
