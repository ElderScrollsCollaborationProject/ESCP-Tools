using System.Collections.Generic;
using Verse;
using RimWorld;
using System.Linq;

namespace ESCP_Spriggan
{
    class IncidentWorker_AnimaSprigganWandersIn : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		IntVec3 intVec;
		return this.TryFindEntryCell(map, out intVec);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		IntVec3 intVec;
		if (!this.TryFindEntryCell(map, out intVec))
		{
			return false;
		}
		PawnKindDef spriggan = PawnKindDef.Named("ESCP_Spriggan_Anima");
		Pawn pawn = null;
		IntVec3 loc = CellFinder.RandomClosewalkCellNear(intVec, map, 10, null);
		pawn = PawnGenerator.GeneratePawn(spriggan, null);
		GenSpawn.Spawn(pawn, loc, map, Rot4.Random, WipeMode.Vanish, false);
		pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);

		Find.LetterStack.ReceiveLetter("ESCP_SprigganWandersIn_Label".Translate(spriggan.label).CapitalizeFirst(), "ESCP_SprigganWandersIn_Description".Translate(spriggan.label), LetterDefOf.ThreatSmall, pawn, null, null);
		return true;
	}

	private bool TryFindEntryCell(Map map, out IntVec3 cell)
	{
		return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal + 0.2f, false, null);
	}
}
}
