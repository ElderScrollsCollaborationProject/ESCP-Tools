using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ESCP.Materials
{
    public class FuelExtension : DefModExtension
    {
        public float fuelValue = 1f;
    }

    [HarmonyPatch(typeof(CompRefuelable), nameof(CompRefuelable.Refuel), typeof(List<Thing>))]
    public static class CompRefuelable_Refuel_FuelValue_Patch
    {
        public static bool Prefix(CompRefuelable __instance, List<Thing> fuelThings)
        {
            if (__instance.Props.atomicFueling || !fuelThings.Any(t => t.def.HasModExtension<FuelExtension>()))
                return true;

            int needed = __instance.GetFuelCountToFullyRefuel();
            while (needed > 0 && fuelThings.Count > 0)
            {
                Thing thing = fuelThings.Pop();
                float value = thing.def.GetModExtension<FuelExtension>()?.fuelValue ?? 1f;
                int used = Mathf.Min(Mathf.CeilToInt(needed / value), thing.stackCount);
                __instance.Refuel(used * value);
                thing.SplitOff(used).Destroy();
                needed -= Mathf.CeilToInt(used * value);
            }
            return false;
        }
    }
}
