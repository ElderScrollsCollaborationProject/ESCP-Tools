using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ESCP.Crafting
{
    [HarmonyPatch(typeof(Bill), nameof(Bill.PawnAllowedToStartAnew))]
    public class Bill_PawnAllowedToStartAnew_Patch
    {
        [HarmonyPostfix]
        public static void RequireXenotype(ref bool __result, Bill __instance, Pawn __0)
        {
            if (!__result)
                return;

            var ext = __instance.recipe.GetModExtension<RecipeExtension_XenotypeCheck>();
            if (ext == null || __0.genes?.Xenotype == ext.requireXenotype)
                return;

            JobFailReason.Is("ESCP_Recipe_XenotypeRequired".Translate(ext.requireXenotype.label));
            __result = false;
        }
    }
}
