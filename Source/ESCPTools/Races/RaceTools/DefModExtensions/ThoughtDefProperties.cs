using System.Collections.Generic;
using Verse;
using RimWorld;

namespace ESCP_RaceTools
{
    public class ThoughtDefProperties : DefModExtension
    {

        public List<string> sharedBackstoryCategories;

        public string backstoryCategoryA;
        public string backstoryCategoryB;

        public TraitDef traitA;
        public TraitDef traitB;

        public bool useSpectrum = false;
        public int traitSpectrumA = 1;
        public int traitSpectrumB = 1;

        public static ThoughtDefProperties Get(Def def)
        {
            return def.GetModExtension<ThoughtDefProperties>();
        }
    }
}
