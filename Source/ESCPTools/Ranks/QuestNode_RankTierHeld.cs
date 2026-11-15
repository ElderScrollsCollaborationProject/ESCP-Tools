using RimWorld.QuestGen;

namespace ESCP.Ranks;

public class QuestNode_RankTierHeld : QuestNode
{
    public SlateRef<RankTierDef> tier;
    public SlateRef<QuestScriptDef> notIfCompleted;

    protected override bool TestRunInt(Slate slate)
    {
        QuestScriptDef done = notIfCompleted.GetValue(slate);
        if (done != null && RankTrackUtility.QuestCompleted(done))
            return false;

        RankTierDef wanted = tier.GetValue(slate);
        GameComponent_PawnRanks comp = Current.Game.GetComponent<GameComponent_PawnRanks>();
        return comp != null && comp.AllTracked.Any(e => e.Value.currentTier == wanted && !e.Key.Dead && e.Key.IsColonist);
    }

    protected override void RunInt()
    {
    }
}
