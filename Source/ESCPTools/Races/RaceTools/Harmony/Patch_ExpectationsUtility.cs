using HarmonyLib;
using RimWorld;
using Verse;
using System;

namespace ESCP_RaceTools
{

    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.LatePatchesCategory)]
    [HarmonyPatch(typeof(ExpectationsUtility))]
    [HarmonyPatch("CurrentExpectationFor")]
    [HarmonyPatch([typeof(Pawn)])]
    public static class ExpectationsUtility_CurrentExpectationFor_Patch
    {
        public static void Postfix(Pawn p, ref ExpectationDef __result)
        {
            if (ESCP_RaceTools_ModSettings.EnableDecreasedExpecations || ESCP_RaceTools_ModSettings.EnableIncreasedExpecations)
            {
                var props = RaceProperties.For(p);
                if (props != null)
                {
                    if (props.modifiedExpectations && ((props.expectationOffset < 0 && ESCP_RaceTools_ModSettings.EnableDecreasedExpecations) || (props.expectationOffset > 0 && ESCP_RaceTools_ModSettings.EnableIncreasedExpecations)))
                    {
                        ExpectationDef expectationDef2 = ExpectationsUtility.ExpectationForOrder(Math.Max(__result.order + props.expectationOffset, 0), true);
                        if (expectationDef2 != null)
                        {
                            __result = expectationDef2;
                        }
                    }
                }
            }
        }
    }
}
