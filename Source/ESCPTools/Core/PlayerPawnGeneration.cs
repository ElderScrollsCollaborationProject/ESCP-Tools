using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP.Core
{
    [HarmonyPatch(typeof(StartingPawnUtility), "DefaultStartingPawnRequest", MethodType.Getter)]
    public static class StartingPawnUtility_DefaultStartingPawnRequest_Patch
    {
        public static void Postfix(ref PawnGenerationRequest __result)
        {
            PawnKindDef kind = __result.KindDef;
            if (kind == null || __result.ForcedXenotype != XenotypeDefOf.Baseliner)
                return;
            if (kind.xenotypeSet != null || kind.useFactionXenotypes && Faction.OfPlayerSilentFail?.def.xenotypeSet != null)
                __result.ForcedXenotype = null;
        }
    }

    [HarmonyPatch(typeof(LifeStageWorker_HumanlikeAdult), nameof(LifeStageWorker_HumanlikeAdult.Notify_LifeStageStarted))]
    public static class LifeStageWorker_HumanlikeAdult_GeneratingPawn_Patch
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn.story != null && pawn.story.Childhood == null)
                pawn.story.Adulthood = null;
        }
    }
}
