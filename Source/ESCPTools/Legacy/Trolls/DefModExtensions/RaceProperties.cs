using Verse;
using System.Collections.Generic;

namespace ESCP_Trolls
{
    public class RaceProperties : DefModExtension
    {

        public bool scaleExtraButcher = false;
        public List<ThingDef> extraButcherItems;

        public static RaceProperties Get(Def def)
        {
            return def.GetModExtension<RaceProperties>();
        }
    }
}
