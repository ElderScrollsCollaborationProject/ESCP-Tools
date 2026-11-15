using HarmonyLib;
using Verse;
using RimWorld;

namespace ESCP_RaceTools
{

    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.LatePatchesCategory)]
    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("GenerateInitialHediffs")]
    public static class PawnGenerator_GenerateInitialHediffs_Patch
    {
        public static void Postfix(Pawn pawn)
        {
            RaceProperties props = RaceProperties.Get(pawn.def);
            if (props != null)
            {
                if (!props.hediffsToAdd.NullOrEmpty())
                {
                    foreach (HediffDef hd in props.hediffsToAdd)
                    {
                        pawn.health.AddHediff(hd);
                    }
                }
                if (!props.abilitiesToAdd.NullOrEmpty())
                {
                    foreach (AbilityDef ad in props.abilitiesToAdd)
                    {
                        pawn.abilities.GainAbility(ad);
                    }
                }
                if (!props.oneOfRandomAbility.NullOrEmpty() && Rand.Chance(props.oneOfRandomAbilityChance))
                {
                    pawn.abilities.GainAbility(props.oneOfRandomAbility.RandomElement());
                }
            }
        }
    }
}
