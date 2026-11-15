using Verse;
using RimWorld;

namespace ESCP_Trolls
{
	public class IncidentWorker_TrollWandersIn : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			return this.TryFindTroll(map, out _) && this.TryFindEntryCell(map, out _);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!this.TryFindTroll(map, out PawnKindDef troll) || !this.TryFindEntryCell(map, out IntVec3 intVec))
			{
				return false;
			}
			if (!RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(intVec, map, 10f, out IntVec3 invalid))
			{
				invalid = IntVec3.Invalid;
			}
			IntVec3 loc = CellFinder.RandomClosewalkCellNear(intVec, map, 10, null);
			Pawn pawn = PawnGenerator.GeneratePawn(troll, null);
			GenSpawn.Spawn(pawn, loc, map, Rot4.Random, WipeMode.Vanish, false);
			if (invalid.IsValid)
			{
				pawn.mindState.forcedGotoPosition = CellFinder.RandomClosewalkCellNear(invalid, map, 10, null);
			}
			Find.LetterStack.ReceiveLetter("ESCP_Trolls_LetterLabelTrollWandersIn".Translate(troll.label).CapitalizeFirst(), "ESCP_Trolls_LetterTrollWandersIn".Translate(troll.label), LetterDefOf.NeutralEvent, pawn, null, null);
			return true;
		}

		private bool TryFindTroll(Map map, out PawnKindDef troll)
		{
			return Utility_OnStartup.trollKindDefs.Where(k => map.Biome.CommonalityOfAnimal(k) > 0f && map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(k.race)).TryRandomElement(out troll);
		}

		private bool TryFindEntryCell(Map map, out IntVec3 cell)
		{
			return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal + 0.2f, false, null);
		}
	}
}
