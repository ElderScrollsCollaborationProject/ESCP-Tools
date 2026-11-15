using HarmonyLib;
using UnityEngine;
using Verse;

namespace ESCP.Core
{
    public class HeadTypeExtension_Skull : DefModExtension
    {
        public HeadTypeDef skull;
    }

    [HarmonyPatch(typeof(PawnRenderNode_Head), nameof(PawnRenderNode_Head.GraphicFor))]
    public static class PawnRenderNode_Head_GraphicFor_Skull_Patch
    {
        public static void Postfix(Pawn pawn, ref Graphic __result)
        {
            if (__result == null || pawn.Drawer.renderer.CurRotDrawMode != RotDrawMode.Dessicated)
                return;
            HeadTypeDef skull = pawn.story?.headType?.GetModExtension<HeadTypeExtension_Skull>()?.skull;
            if (skull != null)
                __result = skull.GetGraphic(pawn, Color.white);
        }
    }
}
