using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP_RaceTools
{

    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.LatePatchesCategory)]
    [HarmonyPatch(typeof(PregnancyUtility))]
    [HarmonyPatch("ApplyBirthOutcome")]
    public static class PregnancyUtility_ApplyBirthOutcome_Patch
    {
        public static void Postfix(ref Thing __result, Pawn father = null)
        {
            if (father != null)
            {
                if (__result != null && __result is Pawn child && !child.Dead)
                {
                    if (father.def != child.def)
                    {
                        RaceProperties props = RaceProperties.Get(father.def);
                        if (props != null)
                        {
                            if (props.majorRacialGeneToInherit != null)
                            {
                                child.genes.AddGene(props.majorRacialGeneToInherit, false);
                            }
                            if (props.minorRacialGeneToInherit != null)
                            {
                                child.genes.AddGene(props.minorRacialGeneToInherit, false);
                            }
                        }
                    }
                }
            }
        }
    }
}
