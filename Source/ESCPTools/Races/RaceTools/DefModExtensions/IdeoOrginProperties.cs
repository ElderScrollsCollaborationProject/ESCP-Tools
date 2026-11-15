using System.Collections.Generic;
using Verse;

namespace ESCP_RaceTools
{

    public class IdeoOrginProperties : DefModExtension
    {
        public bool IgnoreDuplicateDeityNames = true;
        public bool RandomiseDeityName = false;
        public bool RandomiseDeityType = false;

        public bool overrideDeityGenders = false;
        public List<Gender> deityGenders;
        public bool overrideDeityIcons = false;
        public List<string> iconPath;

        public static IdeoOrginProperties Get(Def def)
        {
            return def.GetModExtension<IdeoOrginProperties>();
        }
    }
}
