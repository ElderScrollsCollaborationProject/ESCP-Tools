using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.SketchGen;
using Verse;

namespace ESCP.Materials
{
    public static class GeneratedStuff
    {
        private static List<ThingDef> stone;

        public static List<ThingDef> Stone => stone ??= DefDatabase<ThingDef>.AllDefsListForReading
            .Where(d => d.thingCategories != null && d.thingCategories.Contains(ThingCategoryDefOf.StoneChunks) && d.butcherProducts != null)
            .SelectMany(d => d.butcherProducts).Select(p => p.thingDef)
            .Where(d => d.IsStuff && d.stuffProps.CanMake(ThingDefOf.Wall)).Distinct().ToList();

        public static bool Exploitable(ThingDef stuff)
        {
            return stuff != ThingDefOf.WoodLog && !Stone.Contains(stuff);
        }
    }

    [HarmonyPatch(typeof(BaseGenUtility), nameof(BaseGenUtility.RandomCheapWallStuff), typeof(TechLevel), typeof(bool))]
    public static class GeneratedWalls
    {
        public static void Postfix(ref ThingDef __result)
        {
            if (__result?.stuffProps != null && GeneratedStuff.Exploitable(__result))
                __result = GeneratedStuff.Stone.RandomElementWithFallback(ThingDefOf.WoodLog);
        }
    }

    [HarmonyPatch(typeof(SketchGenUtility), nameof(SketchGenUtility.IsStuffAllowed))]
    public static class GeneratedSketchStuff
    {
        public static void Postfix(ThingDef stuff, ThingDef stuffFor, ref bool __result)
        {
            if (__result && stuffFor?.stuffCategories != null && stuffFor.stuffCategories.Contains(StuffCategoryDefOf.Stony) && GeneratedStuff.Exploitable(stuff))
                __result = false;
        }
    }
}
