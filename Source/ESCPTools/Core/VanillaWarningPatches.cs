using HarmonyLib;

namespace ESCP.Core;

[HarmonyPatch(typeof(IdeoManager), nameof(IdeoManager.Remove))]
public static class IdeoManager_Remove_FactionIdeo_Patch
{
    public static bool Prefix(Ideo ideo, ref bool __result)
    {
        foreach (Faction faction in Find.FactionManager.AllFactions)
        {
            if (faction.ideos != null && faction.ideos.AllIdeos.Contains(ideo))
            {
                __result = false;
                return false;
            }
        }
        return true;
    }
}

[HarmonyPatch(typeof(IdeoManager), nameof(IdeoManager.Add))]
public static class IdeoManager_Add_HiddenRituals_Patch
{
    public static void Postfix(IdeoManager __instance, Ideo ideo)
    {
        List<Precept> precepts = ideo.PreceptsListForReading;
        List<PreceptDef> defs = DefDatabase<PreceptDef>.AllDefsListForReading;
        for (int i = 0; i < defs.Count; i++)
        {
            PreceptDef def = defs[i];
            if (def.visible || !def.preceptClass.SameOrSubclassOf<Precept_Ritual>() || (__instance.classicMode && !def.classic))
                continue;
            if ((def.takeNameFrom == null || precepts.Any(p => p.def == def.takeNameFrom)) && !precepts.Any(p => p.def.ritualPatternBase == def.ritualPatternBase))
                ideo.AddPrecept(PreceptMaker.MakePrecept(def), init: true, null, def.ritualPatternBase);
        }
    }
}

[HarmonyPatch(typeof(HediffComp_RemoveIfApparelDropped), nameof(HediffComp_RemoveIfApparelDropped.CompExposeData))]
public static class HediffComp_RemoveIfApparelDropped_CompExposeData_Patch
{
    public static void Prefix(HediffComp_RemoveIfApparelDropped __instance)
    {
        if (Scribe.mode == LoadSaveMode.Saving && __instance.wornApparel != null && __instance.parent.pawn.apparel?.Wearing(__instance.wornApparel) != true)
            __instance.wornApparel = null;
    }
}
