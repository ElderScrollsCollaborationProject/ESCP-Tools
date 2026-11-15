using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP.Materials
{
    public class MineableExtension_Climate : DefModExtension
    {
        public float maxAverageTemperature = -10f;
        public FloatRange countPer10kCells = new FloatRange(1f, 2f);
    }

    [HarmonyPatch(typeof(GenStep_RocksFromGrid), nameof(GenStep_RocksFromGrid.Generate))]
    public static class GenStep_RocksFromGrid_ClimateMineables_Patch
    {
        public static void Postfix(Map map, GenStepParams parms)
        {
            float temperature = map.TileInfo.temperature;
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                MineableExtension_Climate ext = def.building?.isResourceRock == true ? def.GetModExtension<MineableExtension_Climate>() : null;
                if (ext == null || temperature > ext.maxAverageTemperature)
                    continue;

                new GenStep_ScatterLumpsMineable
                {
                    forcedDefToScatter = def,
                    countPer10kCellsRange = ext.countPer10kCells
                }.Generate(map, parms);
            }
        }
    }
}
