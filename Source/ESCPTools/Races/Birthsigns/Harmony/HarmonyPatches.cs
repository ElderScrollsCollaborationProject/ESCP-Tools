using HarmonyLib;
using Verse;
using System.Reflection;

namespace ESCP_Birthsigns
{
    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.LatePatchesCategory)]
    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("GenerateInitialHediffs")]
    public static class PawnGenerator_GenerateInitialHediffs_Patch
    {
        [HarmonyPostfix]
        public static void BirthsignPatch(Pawn pawn)
        {
            if (!BirthSigns_ModSettings.DisableEntirely && BirthSigns_ModSettings.CurrentSetDef != null && Rand.Chance(BirthSigns_ModSettings.ChanceHasSign))
            {
                if (pawn.RaceProps.Humanlike && (BirthsignExclusion.Get(pawn.def) == null || 
                    (pawn.genes.Xenotype != null && BirthsignExclusion.Get(pawn.genes.Xenotype) == null) || 
                    BirthSigns_ModSettings.AllowDisabledRaces))
                {
                    BirthsignSetDef signs = BirthSigns_ModSettings.CurrentSetDef;
                    BirthSignUtility.GenerateBirthsign(pawn, signs);
                }
            }
        }
    }
}
