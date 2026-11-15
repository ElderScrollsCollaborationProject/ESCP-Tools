using RimWorld.QuestGen;
using UnityEngine;

namespace ESCP.Core.RadiantQuests;

public class QuestNode_GeneratePawns : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;
    public SlateRef<Faction> faction;
    public SlateRef<string> pawnGroupKind;
    public SlateRef<string> pointsToUse;

    protected override bool TestRunInt(Slate slate)
    {
        if (Find.FactionManager.GetFactions().Any(c => c == faction.GetValue(slate)))
        {
            slate.Set(storeAs.GetValue(slate), new List<Pawn>());
            return true;
        }
        return false;
    }

    protected override void RunInt()
    {
        SetVars(QuestGen.slate);
    }

    private void SetVars(Slate slate)
    {
        Faction settlementFaction = faction.GetValue(slate);
        float points = slate.Get("points", 100f);
        if (pointsToUse.TryGetValue(slate, out string pointsString) && float.TryParse(pointsString, out float parsed))
            points = parsed;

        PawnGroupKindDef pawnGroup = DefDatabase<PawnGroupKindDef>.GetNamed(pawnGroupKind.GetValue(slate), false);
        if (pawnGroup == null || !settlementFaction.def.pawnGroupMakers.Any(c => c.kindDef == pawnGroup))
            pawnGroup = PawnGroupKindDefOf.Combat;
        if (settlementFaction.def.pawnGroupMakers.NullOrEmpty() || !settlementFaction.def.pawnGroupMakers.Any(c => c.kindDef == pawnGroup))
        {
            slate.Set(storeAs.GetValue(slate), new List<Pawn>());
            return;
        }
        List<Pawn> Pawns = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
        {
            groupKind = pawnGroup,
            points = Mathf.Max(points, settlementFaction.def.MinPointsToGeneratePawnGroup(pawnGroup)),
            faction = settlementFaction
        }).ToList();
        foreach (Pawn pawn in Pawns)
        {
            if (!Find.WorldPawns.Contains(pawn))
                Find.WorldPawns.PassToWorld(pawn, RimWorld.Planet.PawnDiscardDecideMode.KeepForever);
        }
        slate.Set(storeAs.GetValue(slate), Pawns);
    }
}