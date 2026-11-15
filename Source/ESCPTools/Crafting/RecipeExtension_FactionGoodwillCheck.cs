using RimWorld;
using Verse;

namespace ESCP.Crafting
{
    public class RecipeExtension_FactionGoodwillCheck : DefModExtension
    {
        public FactionDef requireFaction;

        public int minimumGoodwill = -1;

        public bool uncraftableIfFactionDefeated = true;
    }
}
