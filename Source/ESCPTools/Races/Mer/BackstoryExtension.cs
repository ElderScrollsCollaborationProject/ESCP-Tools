using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP_RaceTools_Biotech
{
	public class BackstoryTraitChance
	{
		public TraitDef trait;
		public int degree;
		public float chance = 100f;
	}

	public class BackstoryExtension : DefModExtension
	{
		public float commonality = -1f;
		public List<BackstoryTraitChance> forcedTraitsChance;
		public List<HediffDef> forcedHediffs;
	}

	[StaticConstructorOnStartup]
	public static class BackstoryPatches
	{
		static BackstoryPatches()
		{
			Harmony harmony = new Harmony("ESCP.RaceTools.Biotech");
			harmony.Patch(AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) }),
				postfix: new HarmonyMethod(typeof(BackstoryPatches), nameof(GeneratePawn_Postfix)));

			var weight = AccessTools.Method(typeof(PawnBioAndNameGenerator), "BackstorySelectionWeight");
			if (weight != null)
				harmony.Patch(weight, postfix: new HarmonyMethod(typeof(BackstoryPatches), nameof(SelectionWeight_Postfix)));
			else
				Log.Warning("[ESCP Race Tools] Could not find the backstory weight method, backstory commonality will be ignored.");
		}

		public static void SelectionWeight_Postfix(BackstoryDef bs, ref float __result)
		{
			if (bs.modExtensions == null)
				return;

			float commonality = bs.modExtensions.OfType<BackstoryExtension>().Select(x => x.commonality).LastOrDefault(x => x >= 0f);
			if (commonality > 0f)
				__result *= commonality / 100f;
		}

		public static void GeneratePawn_Postfix(Pawn __result)
		{
			if (__result?.story == null)
				return;

			Apply(__result, __result.story.Childhood);
			Apply(__result, __result.story.Adulthood);
		}

		public static void Apply(Pawn pawn, BackstoryDef backstory)
		{
			if (backstory?.modExtensions == null)
				return;

			foreach (BackstoryExtension extension in backstory.modExtensions.OfType<BackstoryExtension>())
			{
				if (extension.forcedTraitsChance != null)
				{
					foreach (BackstoryTraitChance entry in extension.forcedTraitsChance)
					{
						if (entry.trait != null && !pawn.story.traits.HasTrait(entry.trait) && Rand.Chance(entry.chance / 100f))
							pawn.story.traits.GainTrait(new Trait(entry.trait, entry.degree, true));
					}
				}

				if (extension.forcedHediffs != null)
				{
					foreach (HediffDef hediff in extension.forcedHediffs)
						AddHediff(pawn, hediff);
				}
			}
		}

		private static void AddHediff(Pawn pawn, HediffDef hediff)
		{
			if (pawn.health.hediffSet.HasHediff(hediff))
				return;

			RecipeDef recipe = DefDatabase<RecipeDef>.AllDefsListForReading.FirstOrDefault(x => x.addsHediff == hediff && !x.appliedOnFixedBodyParts.NullOrEmpty());
			BodyPartRecord part = recipe == null ? null : pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault(x => recipe.appliedOnFixedBodyParts.Contains(x.def));
			if (recipe == null || part != null)
				pawn.health.AddHediff(hediff, part);
		}
	}
}
