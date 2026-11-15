using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP_RaceTools_Biotech
{
	public class GeneExtension_Longevity : DefModExtension
	{
		public bool immuneToAge;
		public SimpleCurve adultAgeCurve;
		public SimpleCurve maleFertilityAgeFactor;
		public SimpleCurve femaleFertilityAgeFactor;
	}

	public class PawnRenderNodeWorker_MerRightEar : PawnRenderNodeWorker_FlipWhenCrawling
	{
		public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
		{
			return base.CanDrawNow(node, parms) && LongevityUtility.ShowEar(parms.pawn, "right ear");
		}
	}

	public class PawnRenderNodeWorker_MerLeftEar : PawnRenderNodeWorker_FlipWhenCrawling
	{
		public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
		{
			return base.CanDrawNow(node, parms) && LongevityUtility.ShowEar(parms.pawn, "left ear");
		}
	}

	[StaticConstructorOnStartup]
	public static class LongevityUtility
	{
		private const float AdultAge = 18f;

		static LongevityUtility()
		{
			Harmony harmony = new Harmony("ESCP.RaceTools.Biotech.Longevity");
			harmony.Patch(AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) }),
				postfix: new HarmonyMethod(typeof(LongevityUtility), nameof(GeneratePawn_Postfix)));
			harmony.Patch(AccessTools.Method(typeof(StatPart_FertilityByGenderAge), "AgeFactor"),
				postfix: new HarmonyMethod(typeof(LongevityUtility), nameof(AgeFactor_Postfix)));
			harmony.Patch(AccessTools.Method(typeof(AgeInjuryUtility), nameof(AgeInjuryUtility.GenerateRandomOldAgeInjuries)),
				prefix: new HarmonyMethod(typeof(LongevityUtility), nameof(OldAgeInjuries_Prefix)));
			foreach (var method in AccessTools.GetDeclaredMethods(typeof(AgeInjuryUtility)).Where(x => x.Name == nameof(AgeInjuryUtility.RandomHediffsToGainOnBirthday) && x.GetParameters()[0].ParameterType == typeof(Pawn)))
				harmony.Patch(method, postfix: new HarmonyMethod(typeof(LongevityUtility), nameof(BirthdayHediffs_Postfix)));
		}

		public static GeneExtension_Longevity For(Pawn pawn)
		{
			List<Gene> genes = pawn.genes?.GenesListForReading;
			if (genes == null)
				return null;

			for (int i = 0; i < genes.Count; i++)
			{
				if (genes[i].Active && genes[i].def.modExtensions != null)
				{
					GeneExtension_Longevity extension = genes[i].def.GetModExtension<GeneExtension_Longevity>();
					if (extension != null)
						return extension;
				}
			}
			return null;
		}

		public static bool ShowEar(Pawn pawn, string partLabel)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i] is Hediff_MissingPart && hediffs[i].Part?.customLabel == partLabel)
					return false;
			}

			if (pawn.apparel == null)
				return true;

			List<Apparel> worn = pawn.apparel.WornApparel;
			for (int i = 0; i < worn.Count; i++)
			{
				List<BodyPartGroupDef> groups = worn[i].def.apparel.bodyPartGroups;
				if (groups.Contains(BodyPartGroupDefOf.FullHead) || groups.Contains(BodyPartGroupDefOf.UpperHead))
					return false;
			}
			return true;
		}

		public static void RollAge(Pawn pawn, GeneExtension_Longevity extension)
		{
			if (extension.adultAgeCurve == null || pawn.ageTracker.AgeBiologicalYearsFloat < AdultAge || pawn.story?.Adulthood == null)
				return;

			long ticks = (long)(Rand.ByCurve(extension.adultAgeCurve) * GenDate.TicksPerYear);
			pawn.ageTracker.AgeBiologicalTicks = ticks;
			if (pawn.ageTracker.AgeChronologicalTicks < ticks)
				pawn.ageTracker.AgeChronologicalTicks = ticks;
		}

		public static void GeneratePawn_Postfix(Pawn __result, PawnGenerationRequest request)
		{
			if (__result == null || request.FixedBiologicalAge.HasValue)
				return;

			GeneExtension_Longevity extension = For(__result);
			if (extension != null)
				RollAge(__result, extension);
		}

		public static void AgeFactor_Postfix(Pawn pawn, ref float __result)
		{
			GeneExtension_Longevity extension = For(pawn);
			SimpleCurve curve = pawn.gender == Gender.Female ? extension?.femaleFertilityAgeFactor : extension?.maleFertilityAgeFactor;
			if (curve != null)
				__result = curve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
		}

		public static bool OldAgeInjuries_Prefix(Pawn pawn)
		{
			return For(pawn)?.immuneToAge != true;
		}

		public static void BirthdayHediffs_Postfix(Pawn pawn, ref IEnumerable<HediffGiver_Birthday> __result)
		{
			if (For(pawn)?.immuneToAge == true)
				__result = Enumerable.Empty<HediffGiver_Birthday>();
		}
	}
}
