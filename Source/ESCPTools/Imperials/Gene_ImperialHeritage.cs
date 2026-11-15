using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace ESCP.Imperials
{
    public class Gene_Nibenean : Gene
    {
        private static readonly List<string> AdulthoodCategories = new List<string>
        {
            "ESCP_Backstory_Nibenean_Citizen",
            "ESCP_Backstory_Imperial_Mage",
            "ESCP_Backstory_Imperial_Merchant",
            "ESCP_Backstory_Imperial_Noble",
            "ESCP_Backstory_Imperial_Blades",
            "ESCP_Backstory_Imperial_Emperor",
            "ESCP_Backstory_Imperial_Citizen"
        };

        private static readonly List<string> ChildhoodCategories = new List<string>
        {
            "ESCP_Backstory_Nibenean_Child",
            "ESCP_Backstory_Imperial_Noble_Child",
            "ESCP_Backstory_Imperial_Blades_Child",
            "ESCP_Backstory_Imperial_Emperor_Child",
            "ESCP_Backstory_Imperial_Child"
        };

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn.Spawned) return;
            TryAssignMatchingBackstories(AdulthoodCategories, ChildhoodCategories);
        }

        private bool KindAllows(List<string> adulthoodCats, List<string> childhoodCats)
        {
            List<BackstoryCategoryFilter> filters = pawn.kindDef?.backstoryFiltersOverride;
            if (filters == null)
                return true;
            return filters.Any(f => f.categories?.Any(c => adulthoodCats.Contains(c) || childhoodCats.Contains(c)) == true
                || f.categoriesAdulthood?.Any(adulthoodCats.Contains) == true
                || f.categoriesChildhood?.Any(childhoodCats.Contains) == true);
        }

        private void TryAssignMatchingBackstories(List<string> adulthoodCats, List<string> childhoodCats)
        {
            if (pawn.story == null || !KindAllows(adulthoodCats, childhoodCats)) return;

            if (pawn.story.Adulthood != null && !pawn.story.Adulthood.spawnCategories.Any(adulthoodCats.Contains))
            {
                List<BackstoryDef> candidates = DefDatabase<BackstoryDef>.AllDefsListForReading
                    .Where(b => b.slot == BackstorySlot.Adulthood && b.spawnCategories.Any(adulthoodCats.Contains))
                    .ToList();
                if (candidates.Count > 0)
                    pawn.story.Adulthood = candidates.RandomElement();
            }

            if (pawn.story.Childhood != null && !pawn.story.Childhood.spawnCategories.Any(childhoodCats.Contains))
            {
                List<BackstoryDef> candidates = DefDatabase<BackstoryDef>.AllDefsListForReading
                    .Where(b => b.slot == BackstorySlot.Childhood && b.spawnCategories.Any(childhoodCats.Contains))
                    .ToList();
                if (candidates.Count > 0)
                    pawn.story.Childhood = candidates.RandomElement();
            }
        }
    }

    public class Gene_Colovian : Gene
    {
        private static readonly List<string> AdulthoodCategories = new List<string>
        {
            "ESCP_Backstory_Colovian_Citizen",
            "ESCP_Backstory_Imperial_Legion",
            "ESCP_Backstory_Imperial_Legion_Legatus",
            "ESCP_Backstory_Imperial_Noble",
            "ESCP_Backstory_Imperial_Blades",
            "ESCP_Backstory_Imperial_Emperor",
            "ESCP_Backstory_Imperial_Citizen"
        };

        private static readonly List<string> ChildhoodCategories = new List<string>
        {
            "ESCP_Backstory_Colovian_Child",
            "ESCP_Backstory_Imperial_Noble_Child",
            "ESCP_Backstory_Imperial_Blades_Child",
            "ESCP_Backstory_Imperial_Emperor_Child",
            "ESCP_Backstory_Imperial_Child"
        };

        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn.Spawned) return;
            TryAssignMatchingBackstories(AdulthoodCategories, ChildhoodCategories);
        }

        private bool KindAllows(List<string> adulthoodCats, List<string> childhoodCats)
        {
            List<BackstoryCategoryFilter> filters = pawn.kindDef?.backstoryFiltersOverride;
            if (filters == null)
                return true;
            return filters.Any(f => f.categories?.Any(c => adulthoodCats.Contains(c) || childhoodCats.Contains(c)) == true
                || f.categoriesAdulthood?.Any(adulthoodCats.Contains) == true
                || f.categoriesChildhood?.Any(childhoodCats.Contains) == true);
        }

        private void TryAssignMatchingBackstories(List<string> adulthoodCats, List<string> childhoodCats)
        {
            if (pawn.story == null || !KindAllows(adulthoodCats, childhoodCats)) return;

            if (pawn.story.Adulthood != null && !pawn.story.Adulthood.spawnCategories.Any(adulthoodCats.Contains))
            {
                List<BackstoryDef> candidates = DefDatabase<BackstoryDef>.AllDefsListForReading
                    .Where(b => b.slot == BackstorySlot.Adulthood && b.spawnCategories.Any(adulthoodCats.Contains))
                    .ToList();
                if (candidates.Count > 0)
                    pawn.story.Adulthood = candidates.RandomElement();
            }

            if (pawn.story.Childhood != null && !pawn.story.Childhood.spawnCategories.Any(childhoodCats.Contains))
            {
                List<BackstoryDef> candidates = DefDatabase<BackstoryDef>.AllDefsListForReading
                    .Where(b => b.slot == BackstorySlot.Childhood && b.spawnCategories.Any(childhoodCats.Contains))
                    .ToList();
                if (candidates.Count > 0)
                    pawn.story.Childhood = candidates.RandomElement();
            }
        }
    }

    public enum ImperialHeritageType
    {
        Random,
        Nibenean,
        Colovian
    }

    public class ImperialHeritageExtension : DefModExtension
    {
        public ImperialHeritageType heritage = ImperialHeritageType.Random;
    }

    public class Gene_ImperialHeritage : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            if (pawn.Spawned) return;

            ImperialHeritageType type = ImperialHeritageType.Random;
            ImperialHeritageExtension ext = pawn.kindDef?.GetModExtension<ImperialHeritageExtension>() as ImperialHeritageExtension;
            if (ext != null)
                type = ext.heritage;

            GeneDef chosen;
            if (type == ImperialHeritageType.Nibenean)
                chosen = DefDatabase<GeneDef>.GetNamed("ESCP_Gene_Nibenean");
            else if (type == ImperialHeritageType.Colovian)
                chosen = DefDatabase<GeneDef>.GetNamed("ESCP_Gene_Colovian");
            else
                chosen = Rand.Bool
                    ? DefDatabase<GeneDef>.GetNamed("ESCP_Gene_Nibenean")
                    : DefDatabase<GeneDef>.GetNamed("ESCP_Gene_Colovian");

            pawn.genes.AddGene(chosen, true);
            pawn.genes.RemoveGene(this);
        }
    }
}
