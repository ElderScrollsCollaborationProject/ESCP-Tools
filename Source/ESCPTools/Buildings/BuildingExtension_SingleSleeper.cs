using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP.Buildings
{
    public class BuildingExtension_SingleSleeper : DefModExtension
    {
    }

    [HarmonyPatch(typeof(Building_Bed), nameof(Building_Bed.SleepingSlotsCount), MethodType.Getter)]
    public static class Building_Bed_SleepingSlotsCount_Patch
    {
        public static void Postfix(ref int __result, Building_Bed __instance)
        {
            if (__result > 1 && __instance.def.HasModExtension<BuildingExtension_SingleSleeper>())
                __result = 1;
        }
    }

    [HarmonyPatch(typeof(Building_Bed), nameof(Building_Bed.GetSleepingSlotPos))]
    public static class Building_Bed_GetSleepingSlotPos_Patch
    {
        public static void Postfix(ref IntVec3 __result, Building_Bed __instance)
        {
            if (__instance.def.HasModExtension<BuildingExtension_SingleSleeper>())
                __result = __instance.Position;
        }
    }
}
