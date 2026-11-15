using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace ESCP_RaceTools
{
    public class RomanceCheck
    {
        public static bool CanRomance(Pawn initiator, Pawn target, out string opinionExplanation)
        {
            opinionExplanation = null;
            RomanceRestriction propsInitiator = RomanceRestriction.Get(initiator.def);
            RomanceRestriction propsTarget = RomanceRestriction.Get(target.def);

            if (propsInitiator == null && propsTarget == null)
            {
                return true;
            }

            bool propsNullFlag = false;
            bool romanceRestrictedFlag = false;

            if (propsInitiator == null)
            {
                propsNullFlag = true;
            }
            else
            {
                if (propsInitiator.restrictRomanceToTags)
                {
                    romanceRestrictedFlag = true;
                }
            }
            if (propsTarget == null)
            {
                propsNullFlag = true;
            }
            else
            {
                if (propsTarget.restrictRomanceToTags)
                {
                    romanceRestrictedFlag = true;
                }
            }

            if (!romanceRestrictedFlag)
            {
                return true;
            }

            if (romanceRestrictedFlag && propsNullFlag)
            {
                opinionExplanation = "ESCP_RomanceRestricted".Translate(initiator.def.label, target.def.label);
                return false;
            }

            if (propsInitiator.restrictedRomanceTags.NullOrEmpty() || propsTarget.restrictedRomanceTags.NullOrEmpty())
            {
                return false;
            }

            return propsInitiator.restrictedRomanceTags.Intersect(propsTarget.restrictedRomanceTags).Any();
        }
    }

    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.LatePatchesCategory)]
    [HarmonyPatch(typeof(RelationsUtility))]
    [HarmonyPatch("RomanceEligiblePair")]
    public static class RelationsUtility_RomanceEligiblePair_Patch
    {
        public static void Postfix(Pawn initiator, Pawn target, bool forOpinionExplanation, ref AcceptanceReport __result)
        {
            if (__result)
            {
                bool canRomance = RomanceCheck.CanRomance(initiator, target, out string opinionExplanation);
                if (!canRomance && forOpinionExplanation)
                {

                    __result = opinionExplanation;
                    return;
                }
                __result = canRomance;
            }
        }
    }

    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.LatePatchesCategory)]
    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt))]
    [HarmonyPatch("RandomSelectionWeight")]
    public static class InteractionWorker_RomanceAttempt_RandomSelectionWeight_Patch
    {
        public static void Postfix(Pawn initiator, Pawn recipient, ref float __result)
        {
            if (__result > 0f)
            {
                bool canRomance = RomanceCheck.CanRomance(initiator, recipient, out string _);
                if (!canRomance)
                {
                    __result = 0f;
                }
            }
        }
    }

    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.LatePatchesCategory)]
    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt))]
    [HarmonyPatch("SuccessChance")]
    public static class InteractionWorker_RomanceAttempt_SuccessChance_Patch
    {
        public static void Postfix(Pawn initiator, Pawn recipient, ref float __result)
        {
            if (__result > 0f)
            {
                bool canRomance = RomanceCheck.CanRomance(initiator, recipient, out string _);
                if (!canRomance)
                {
                    __result = 0f;
                }
            }
        }
    }
}