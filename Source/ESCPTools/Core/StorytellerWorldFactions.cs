using HarmonyLib;

namespace ESCP.Core;

public class StorytellerExtension_WorldFactions : DefModExtension
{
    public TechLevel maxTechLevel = TechLevel.Archotech;
    public List<FactionDef> disabledFactions = new List<FactionDef>();
    public List<GenStepDef> disabledGenSteps = new List<GenStepDef>();

    public bool Disables(FactionDef faction)
    {
        return disabledFactions.Contains(faction) || (!faction.hidden && faction.techLevel > maxTechLevel);
    }
}

[HarmonyPatch(typeof(Page_CreateWorldParams), "ResetFactionCounts")]
public static class Page_CreateWorldParams_ResetFactionCounts_Patch
{
    public static void Postfix(List<FactionDef> ___factions, List<FactionDef> ___initialFactions)
    {
        StorytellerExtension_WorldFactions ext = Current.Game?.storyteller?.def?.GetModExtension<StorytellerExtension_WorldFactions>();
        if (ext == null)
            return;

        ___factions.RemoveAll(ext.Disables);
        ___initialFactions.RemoveAll(ext.Disables);
    }
}

[HarmonyPatch(typeof(Page_CreateWorldParams), nameof(Page_CreateWorldParams.PreOpen))]
public static class Page_CreateWorldParams_PreOpen_Patch
{
    private static StorytellerDef appliedFor;

    public static void Postfix(Page_CreateWorldParams __instance)
    {
        StorytellerDef current = Current.Game?.storyteller?.def;
        if (current == appliedFor)
            return;

        appliedFor = current;
        AccessTools.Method(typeof(Page_CreateWorldParams), "ResetFactionCounts").Invoke(__instance, null);
    }
}

[HarmonyPatch(typeof(MapGenerator), nameof(MapGenerator.GenerateContentsIntoMap))]
public static class MapGenerator_GenerateContentsIntoMap_Patch
{
    public static void Prefix(ref IEnumerable<GenStepWithParams> genStepDefs)
    {
        List<GenStepDef> disabled = Current.Game?.storyteller?.def?.GetModExtension<StorytellerExtension_WorldFactions>()?.disabledGenSteps;
        if (!disabled.NullOrEmpty())
            genStepDefs = genStepDefs.Where(s => !disabled.Contains(s.def)).ToList();
    }
}
