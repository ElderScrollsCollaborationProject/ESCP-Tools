using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP.Buildings
{
    public class BuildingExtension_NoStacking : DefModExtension
    {
    }

    [HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanPlaceBlueprintAt))]
    public static class GenConstruct_CanPlaceBlueprintAt_NoStacking_Patch
    {
        public static void Postfix(ref AcceptanceReport __result, BuildableDef entDef, IntVec3 center, Rot4 rot, Map map)
        {
            if (!__result.Accepted || !entDef.HasModExtension<BuildingExtension_NoStacking>())
                return;

            foreach (IntVec3 cell in GenAdj.OccupiedRect(center, rot, entDef.Size))
            {
                List<Thing> things = cell.GetThingList(map);
                for (int i = 0; i < things.Count; i++)
                {
                    BuildableDef def = things[i].def.entityDefToBuild ?? things[i].def;
                    if (def.HasModExtension<BuildingExtension_NoStacking>())
                    {
                        __result = "SpaceAlreadyOccupied".Translate();
                        return;
                    }
                }
            }
        }
    }
}
