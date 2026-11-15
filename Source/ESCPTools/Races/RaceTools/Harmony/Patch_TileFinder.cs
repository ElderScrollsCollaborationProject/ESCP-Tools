using HarmonyLib;
using RimWorld;
using Verse;
using RimWorld.Planet;
using System;

namespace ESCP_RaceTools
{

    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.LatePatchesCategory)]
    [HarmonyPatch(typeof(TileFinder))]
    [HarmonyPatch("RandomSettlementTileFor")]
    [HarmonyPatch([typeof(PlanetLayer), typeof(Faction), typeof(bool), typeof(Predicate<PlanetTile>)])]

    public static class TileFinder_RandomSettlementTileFor_Patch
    {
        public static bool Prefix(PlanetLayer layer, Faction faction, ref PlanetTile __result)
        {
            if (ESCP_RaceTools_ModSettings.EnableSettlementPreference)
            {
                if (faction != null && SettlementPreference.Get(faction.def) != null && Rand.Chance(SettlementPreference.Get(faction.def).chance))
                {

                    return SettlementPreferenceUtility.GetTileID(layer, faction, out __result);
                }
            }
            return true;
        }
    }
}
