using Verse;

namespace ESCP_RaceTools
{
    public class PlantProperties : DefModExtension
    {

        public ThingDef matureInto = null;

        public static PlantProperties Get(Def def)
        {
            return def.GetModExtension<PlantProperties>();
        }
    }
}
