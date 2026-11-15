using Verse;

namespace ESCP_Birthsigns
{

    class BirthsignExclusion : DefModExtension
    {
        public static BirthsignExclusion Get(Def def)
        {
            return def.GetModExtension<BirthsignExclusion>();
        }
    }
}
