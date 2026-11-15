using RimWorld;
using Verse;

namespace ESCP.Materials
{
    public class ButcherExtension : DefModExtension
    {
        public bool hasBone = true;
        public bool hasChitin;
        public bool hasFat = true;
    }

    [DefOf]
    public static class MaterialDefOf
    {
        public static ThingDef ESCP_Item_Bone;
        public static ThingDef ESCP_Item_Chitin;
        public static ThingDef ESCP_Item_Fat;

        static MaterialDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MaterialDefOf));
        }
    }

    [StaticConstructorOnStartup]
    public static class ButcherExtras
    {
        static ButcherExtras()
        {
            if (DefDatabase<Core.ModOptionDef>.GetNamed("ESCP_Option_ButcherExtras").Enabled)
                AddButcherExtras();
        }

        private static void AddButcherExtras()
        {
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.race == null || !def.race.IsFlesh)
                    continue;

                ButcherExtension ext = def.GetModExtension<ButcherExtension>();
                if (ext == null && !def.race.Animal)
                    continue;

                bool insect = def.race.FleshType == FleshTypeDefOf.Insectoid;
                float meat = def.GetStatValueAbstract(StatDefOf.MeatAmount);
                if (ext?.hasChitin ?? insect)
                    Add(def, MaterialDefOf.ESCP_Item_Chitin, meat / 5f);
                if (ext?.hasBone ?? !insect)
                    Add(def, MaterialDefOf.ESCP_Item_Bone, meat / 5f);
                if (ext?.hasFat ?? true)
                    Add(def, MaterialDefOf.ESCP_Item_Fat, meat / 9f);
            }
        }

        private static void Add(ThingDef def, ThingDef product, float amount)
        {
            int count = (int)amount;
            if (count < 1)
                return;

            def.butcherProducts ??= new List<ThingDefCountClass>();
            if (!def.butcherProducts.Any(p => p.thingDef == product))
                def.butcherProducts.Add(new ThingDefCountClass(product, count));
        }
    }
}
