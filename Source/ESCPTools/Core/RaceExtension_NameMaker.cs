using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP.Core
{
    public class RaceExtension_NameMaker : DefModExtension
    {
        public RulePackDef nameMaker;
    }

    [HarmonyPatch(typeof(PawnBioAndNameGenerator), nameof(PawnBioAndNameGenerator.GeneratePawnName))]
    public static class PawnBioAndNameGenerator_GeneratePawnName_RaceNames_Patch
    {
        public static bool Prefix(ref Name __result, Pawn pawn)
        {
            RulePackDef maker = pawn.def.GetModExtension<RaceExtension_NameMaker>()?.nameMaker;
            if (maker == null)
                return true;

            __result = NameTriple.FromString(NameGenerator.GenerateName(maker));
            return false;
        }
    }
}
