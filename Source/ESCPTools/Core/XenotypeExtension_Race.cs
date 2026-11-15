using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP.Core
{
    public class XenotypeExtension_Race : DefModExtension
    {
        public PawnKindDef pawnKind;
    }

    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), typeof(PawnGenerationRequest))]
    public static class PawnGenerator_GeneratePawn_XenotypeRace_Patch
    {
        public static void Prefix(ref PawnGenerationRequest request)
        {
            PawnKindDef kind = request.ForcedXenotype?.GetModExtension<XenotypeExtension_Race>()?.pawnKind;
            if (kind != null && request.KindDef?.race != kind.race)
            {
                request.PawnKindDefGetter = null;
                request.KindDef = kind;
            }
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GetXenotypeForGeneratedPawn))]
    public static class PawnGenerator_GetXenotypeForGeneratedPawn_XenotypeRace_Patch
    {
        public static void Postfix(ref XenotypeDef __result, PawnGenerationRequest request)
        {
            PawnKindDef kind = __result?.GetModExtension<XenotypeExtension_Race>()?.pawnKind;
            if (kind != null && request.KindDef?.race != kind.race)
                __result = XenotypeDefOf.Baseliner;
        }
    }
}
