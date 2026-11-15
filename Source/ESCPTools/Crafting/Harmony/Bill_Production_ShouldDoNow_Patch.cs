using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP.Crafting
{
    [HarmonyPatch(typeof(Bill_Production), "ShouldDoNow")]
    public class Bill_Production_ShouldDoNow_Patch
    {
        [HarmonyPrefix]
        public static void ShouldDoNow_Checks(ref bool __result, Bill_Production __instance)
        {
            CheckGoodwill(ref __result, __instance);
            CheckXenotype(ref __result, __instance);
        }

        private static void CheckGoodwill(ref bool __result, Bill_Production __instance)
        {
            var ext = __instance.recipe.GetModExtension<RecipeExtension_FactionGoodwillCheck>();
            if (ext == null)
                return;

            Faction faction = Find.FactionManager.FirstFactionOfDef(ext.requireFaction);
            if (faction == null)
            {
                if (!__instance.suspended)
                    Messages.Message("ESCP_Recipe_FactionMissing".Translate(ext.requireFaction.defName), MessageTypeDefOf.NegativeEvent);
                __instance.suspended = true;
                __result = false;
                return;
            }

            if (faction.defeated && ext.uncraftableIfFactionDefeated)
            {
                if (!__instance.suspended)
                    Messages.Message("ESCP_Recipe_FactionDefeated".Translate(faction.Name), MessageTypeDefOf.NegativeEvent);
                __instance.suspended = true;
                __result = false;
                return;
            }

            if (faction.GoodwillWith(Faction.OfPlayer) < ext.minimumGoodwill)
            {
                if (!__instance.suspended)
                    Messages.Message("ESCP_Recipe_GoodwillUnmet".Translate(faction.Name, ext.minimumGoodwill), MessageTypeDefOf.NegativeEvent);
                __instance.suspended = true;
                __result = false;
            }
        }

        private static void CheckXenotype(ref bool __result, Bill_Production __instance)
        {
            var ext = __instance.recipe.GetModExtension<RecipeExtension_XenotypeCheck>();
            if (ext == null)
                return;

            bool eligible = false;
            foreach (Pawn p in __instance.billStack?.billGiver?.Map?.mapPawns?.FreeColonists ?? new System.Collections.Generic.List<Pawn>())
            {
                if (p.genes?.Xenotype == ext.requireXenotype && p.workSettings?.WorkIsActive(WorkTypeDefOf.Crafting) == true)
                {
                    eligible = true;
                    break;
                }
            }

            if (!eligible)
            {
                if (!__instance.suspended)
                    Messages.Message("ESCP_Recipe_XenotypeRequired".Translate(ext.requireXenotype.label), MessageTypeDefOf.NegativeEvent);
                __instance.suspended = true;
                __result = false;
            }
        }
    }
}
