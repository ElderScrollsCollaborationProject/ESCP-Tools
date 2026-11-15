using HarmonyLib;

namespace ESCP.Core;

public class ThoughtExtension_XenotypeDislike : DefModExtension
{
    public XenotypeDef xenotype;
    public List<XenotypeDef> dislikes;
    public float indifferentChance = 0.1f;
}

public class ThoughtWorker_XenotypeDislike : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
    {
        ThoughtExtension_XenotypeDislike extension = def.GetModExtension<ThoughtExtension_XenotypeDislike>();
        if (pawn.genes?.Xenotype != extension.xenotype || other.genes?.Xenotype == null || !extension.dislikes.Contains(other.genes.Xenotype))
            return ThoughtState.Inactive;

        return Rand.ChanceSeeded(extension.indifferentChance, pawn.thingIDNumber) ? ThoughtState.Inactive : ThoughtState.ActiveAtStage(0);
    }
}

public static class IdeoXenotypeUtility
{
    public static bool Rejects(Pawn pawn, Ideo ideo)
    {
        XenotypeDef xenotype = pawn.genes?.Xenotype;
        if (xenotype == null || ideo == null)
            return false;

        List<MemeDef> memes = ideo.memes;
        for (int i = 0; i < memes.Count; i++)
        {
            List<XenotypeDef> blacklist = memes[i].GetModExtension<XenotypeExtension>()?.blacklistedXenotypes;
            if (blacklist != null && blacklist.Contains(xenotype))
                return true;
        }
        return false;
    }
}

[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.IdeoConversionAttempt))]
public static class Pawn_IdeoTracker_IdeoConversionAttempt_Patch
{
    public static bool Prefix(Pawn ___pawn, Ideo initiatorIdeo, ref bool __result)
    {
        if (!IdeoXenotypeUtility.Rejects(___pawn, initiatorIdeo))
            return true;
        __result = false;
        return false;
    }
}

[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.SetIdeo))]
public static class Pawn_IdeoTracker_SetIdeo_Patch
{
    public static bool Prefix(Pawn_IdeoTracker __instance, Pawn ___pawn, Ideo ideo)
    {
        return __instance.Ideo == null || !IdeoXenotypeUtility.Rejects(___pawn, ideo);
    }
}

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(GeneUIUtility), "DrawGeneBasics")]
public static class GeneUIUtility_DrawGeneBasics_Patch
{
    public static readonly CachedTexture Endogene = new CachedTexture("UI/Icons/Genes/ESCP_GeneBackground_Endogenes");
    public static readonly CachedTexture Xenogene = new CachedTexture("UI/Icons/Genes/ESCP_GeneBackground_Xenogenes");

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.LoadsField(AccessTools.Field(typeof(GeneUIUtility), "GeneBackground_Endogene")))
                instruction.operand = AccessTools.Field(typeof(GeneUIUtility_DrawGeneBasics_Patch), nameof(Endogene));
            else if (instruction.LoadsField(AccessTools.Field(typeof(GeneUIUtility), "GeneBackground_Xenogene")))
                instruction.operand = AccessTools.Field(typeof(GeneUIUtility_DrawGeneBasics_Patch), nameof(Xenogene));
            yield return instruction;
        }
    }
}
