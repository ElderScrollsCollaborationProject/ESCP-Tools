using HarmonyLib;

namespace ESCP.Core;

public class ScenarioEras : DefModExtension
{
    public List<int> eras = new List<int>();
}

[HarmonyPatch(typeof(ScenarioLister), nameof(ScenarioLister.ScenariosInCategory))]
public static class ScenarioLister_ScenariosInCategory_Era_Patch
{
    public static IEnumerable<Scenario> Postfix(IEnumerable<Scenario> scenarios, ScenarioCategory cat)
    {
        if (cat != ScenarioCategory.FromDef)
            return scenarios;

        HashSet<Scenario> hidden = new HashSet<Scenario>();
        foreach (ScenarioDef def in DefDatabase<ScenarioDef>.AllDefsListForReading)
        {
            ScenarioEras ext = def.GetModExtension<ScenarioEras>();
            if (ext != null && !ESCPToolsMod.Settings.AllowsEras(ext.eras))
                hidden.Add(def.scenario);
        }
        return hidden.Count == 0 ? scenarios : scenarios.Where(s => !hidden.Contains(s));
    }
}
