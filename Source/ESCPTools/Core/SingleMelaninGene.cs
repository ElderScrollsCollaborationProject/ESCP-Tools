using HarmonyLib;

namespace ESCP.Core;

public static class SingleMelaninGene
{
    public static void Trim(Pawn pawn, bool keepCurrentTone)
    {
        bool escp = pawn.genes.Xenotype != null && pawn.genes.Xenotype.defName.StartsWith("ESCP_");
        List<Gene> escpSkins = pawn.genes.GenesListForReading.Where(g => g.def.skinColorOverride.HasValue && g.def.defName.StartsWith("ESCP_")).ToList();
        if (!escp || escpSkins.Count > 1)
        {
            Gene keepSkin = !escp ? null : keepCurrentTone ? escpSkins.FirstOrDefault(g => g.def.skinColorOverride == pawn.story?.skinColorOverride) : null;
            if (escp)
                keepSkin ??= escpSkins.RandomElement();
            for (int i = 0; i < escpSkins.Count; i++)
            {
                if (escpSkins[i] != keepSkin)
                    pawn.genes.RemoveGene(escpSkins[i]);
            }
        }

        List<Gene> genes = pawn.genes.GenesListForReading;
        List<Gene> melanin = genes.Where(g => g.def.endogeneCategory == EndogeneCategory.Melanin).ToList();
        if (melanin.Count == 0)
            return;

        Gene keep = null;
        bool overridden = escp && genes.Any(g => g.def.skinColorOverride.HasValue);
        if (!overridden)
        {
            keep = keepCurrentTone ? melanin.FirstOrDefault(g => g.def.skinColorBase == pawn.story?.SkinColorBase) : null;
            keep ??= melanin.RandomElement();
        }

        for (int i = 0; i < melanin.Count; i++)
        {
            if (melanin[i] != keep)
                pawn.genes.RemoveGene(melanin[i]);
        }
    }
}

[HarmonyPatch(typeof(PawnGenerator), "GenerateGenes")]
public static class PawnGenerator_GenerateGenes_SingleMelanin_Patch
{
    public static void Postfix(Pawn pawn)
    {
        if (pawn.genes == null)
            return;
        RacialGenes.Strip(pawn);
        SingleMelaninGene.Trim(pawn, false);
    }
}

[HarmonyPatch(typeof(Pawn_GeneTracker), nameof(Pawn_GeneTracker.ExposeData))]
public static class Pawn_GeneTracker_ExposeData_SingleMelanin_Patch
{
    public static void Postfix(Pawn_GeneTracker __instance)
    {
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                RacialGenes.Strip(__instance.pawn);
                SingleMelaninGene.Trim(__instance.pawn, true);
            });
        }
    }
}
