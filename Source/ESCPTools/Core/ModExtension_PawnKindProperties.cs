using Verse;

namespace ESCP.Core
{
    public class ModExtension_PawnKindProperties : DefModExtension
    {
        public bool purchasableFromTrader = false;

        public static ModExtension_PawnKindProperties Get(Def def)
        {
            return def.GetModExtension<ModExtension_PawnKindProperties>();
        }
    }
}
