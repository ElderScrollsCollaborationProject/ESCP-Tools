using RimWorld;
using Verse;

namespace ESCP.Materials
{
    [StaticConstructorOnStartup]
    public static class VanillaOres
    {
        static VanillaOres()
        {
            if (!DefDatabase<Core.ModOptionDef>.GetNamed("ESCP_Option_NoIndustrialOres").Enabled)
                return;

            ThingDefOf.Steel.deepCommonality = 0f;
            List<ThingDef> lumps = (GenStepDefOf.PreciousLump.genStep as GenStep_PreciousLump)?.mineables;
            foreach (string defName in new[] { "MineableSteel", "MineableComponentsIndustrial" })
            {
                ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                if (def?.building == null)
                    continue;
                def.building.mineableScatterCommonality = 0f;
                lumps?.Remove(def);
            }
        }
    }
}
