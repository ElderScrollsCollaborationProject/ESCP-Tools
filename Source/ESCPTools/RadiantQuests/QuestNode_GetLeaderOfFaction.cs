using RimWorld.QuestGen;

namespace ESCP.Core.RadiantQuests;

public class QuestNode_GetLeaderOfFaction : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<FactionDef> factionDef = null;
    public SlateRef<Faction> faction = null;
    public SlateRef<Faction> factionToUse = null;

    public SlateRef<bool> factionMustBePermanent = true;

    protected override bool TestRunInt(Slate slate)
    {
        return GetFactionLeader(ResolveFaction(slate)) != null;
    }

    private Faction ResolveFaction(Slate slate)
    {
        if (factionToUse != null && factionToUse.TryGetValue(slate, out Faction ftu) && ftu != null)
            return ftu;
        if (faction != null && faction.TryGetValue(slate, out Faction f) && f != null)
            return f;
        return TryFindFaction(out Faction found, slate) ? found : null;
    }

    private bool TryFindFaction(out Faction outFaction, Slate slate)
    {
        outFaction = null;
        if (!factionDef.TryGetValue(slate, out FactionDef def) || def == null)
        {
            return false;
        }
        return Find.FactionManager.GetFactions().Where(c => c.def == def).TryRandomElement(out outFaction);
    }

    protected override void RunInt()
    {
        SetVars(QuestGen.slate);
    }

    private void SetVars(Slate slate)
    {
        Pawn pawn = GetFactionLeader(ResolveFaction(slate));
        if (pawn == null)
        {
            ESCPLog.Error("Failed to get faction leader for quest");
            return;
        }

        QuestGen.slate.Set(storeAs.GetValue(slate), pawn);
    }

    private Pawn GetFactionLeader(Faction faction)
    {
        return faction?.leader != null && !faction.leader.Dead ? faction.leader : null;
    }

}