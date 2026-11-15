using System.Collections.Generic;
using Verse;

namespace ESCP_RaceTools
{
    public class FactionProperties : DefModExtension
    {

        public List<string> factionTags;

        public List<string> hireableFactionTags;

        public static FactionProperties Get(Def def)
        {
            return def.GetModExtension<FactionProperties>();
        }
    }
}
