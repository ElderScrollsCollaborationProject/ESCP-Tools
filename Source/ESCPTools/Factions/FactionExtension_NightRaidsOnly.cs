using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP.Factions
{
    public class FactionExtension_NightRaidsOnly : DefModExtension
    {
        public float maxSkyGlow = 0.4f;
    }

    [HarmonyPatch(typeof(IncidentWorker_PawnsArrive), nameof(IncidentWorker_PawnsArrive.FactionCanBeGroupSource))]
    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.LatePatchesCategory)]
    public static class IncidentWorker_PawnsArrive_FactionCanBeGroupSource_NightOnly_Patch
    {
        public static void Postfix(ref bool __result, Faction f, IncidentParms parms)
        {
            if (__result && f.def.GetModExtension<FactionExtension_NightRaidsOnly>() is { } ext && parms.target is Map map && map.skyManager.CurSkyGlow > ext.maxSkyGlow)
                __result = false;
        }
    }
}
