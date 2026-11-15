using HarmonyLib;
using UnityEngine;

namespace ESCP_RaceTools_Biotech;

public class GeneExtension_EyeColor : DefModExtension
{
    public Color color = Color.red;

    public static GeneExtension_EyeColor For(Pawn pawn)
    {
        List<Gene> genes = pawn.genes?.GenesListForReading;
        if (genes == null)
            return null;

        for (int i = 0; i < genes.Count; i++)
        {
            if (genes[i].Active && genes[i].def.modExtensions != null)
            {
                GeneExtension_EyeColor extension = genes[i].def.GetModExtension<GeneExtension_EyeColor>();
                if (extension != null)
                    return extension;
            }
        }
        return null;
    }
}

[HarmonyPatch(typeof(PawnRenderNode_Head), nameof(PawnRenderNode_Head.GraphicFor))]
public static class PawnRenderNode_Head_GraphicFor_Patch
{
    public static void Postfix(Pawn pawn, ref Graphic __result)
    {
        if (__result == null || pawn.story?.headType == null || __result.path != pawn.story.headType.graphicPath)
            return;

        GeneExtension_EyeColor extension = GeneExtension_EyeColor.For(pawn);
        if (extension != null)
            __result = GraphicDatabase.Get<Graphic_Multi>(__result.path, ShaderDatabase.CutoutComplex, __result.drawSize, __result.color, extension.color);
    }
}
