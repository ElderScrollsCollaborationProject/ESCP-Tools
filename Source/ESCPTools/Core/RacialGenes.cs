using HarmonyLib;

namespace ESCP.Core;

[StaticConstructorOnStartup]
public static class RacialGenes
{
    private static readonly HashSet<GeneDef> racial = new HashSet<GeneDef>();

    static RacialGenes()
    {
        foreach (XenotypeDef xenotype in DefDatabase<XenotypeDef>.AllDefsListForReading)
        {
            if (!xenotype.defName.StartsWith("ESCP_"))
                continue;
            foreach (GeneDef gene in xenotype.genes)
            {
                if (gene.defName.StartsWith("ESCP_"))
                    racial.Add(gene);
            }
        }
        foreach (GeneDef gene in racial)
            gene.canGenerateInGeneSet = false;
    }

    public static void Strip(Pawn pawn)
    {
        List<Gene> genes = pawn.genes.GenesListForReading;
        List<GeneDef> allowed = pawn.genes.UniqueXenotype ? null : pawn.genes.Xenotype.genes;
        for (int i = genes.Count - 1; i >= 0; i--)
        {
            if (racial.Contains(genes[i].def) && (allowed == null || !allowed.Contains(genes[i].def)))
                pawn.genes.RemoveGene(genes[i]);
        }
    }
}

[HarmonyPatch(typeof(PregnancyUtility), nameof(PregnancyUtility.ApplyBirthOutcome))]
[HarmonyPatchCategory(ESCPToolsMod.LatePatchesCategory)]
public static class PregnancyUtility_ApplyBirthOutcome_RacialGenes_Patch
{
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(Thing __result)
    {
        if (__result is Pawn { genes: not null } baby)
            RacialGenes.Strip(baby);
    }
}
