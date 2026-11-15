using RimWorld;
using Verse;

namespace ESCP.Currencies
{
    public class CurrencyReplacement : DefModExtension
    {
        public ThingDef currency;

        public static CurrencyReplacement Get(Def def)
        {
            return def.GetModExtension<CurrencyReplacement>();
        }
    }
}
