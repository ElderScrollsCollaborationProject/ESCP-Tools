namespace ESCP.Ranks.Jobs;

public class JobDriver_RankStudying : JobDriver_RankSession
{
    protected override JobDef PartnerJobDef => RankJobDefOf.ESCP_Job_RankStudyingPartner;

    protected override void TickAction()
    {
        GrantXp(pawn, RankComp?.GetRankData(pawn)?.currentTier?.trainingXpPerHour, 0.5f);
        GrantXp(Partner, RankComp?.GetRankData(Partner)?.currentTier?.trainingXpPerHour, 0.5f);
    }

    protected override void Complete()
    {
        GameComponent_PawnRanks comp = RankComp;
        int now = Find.TickManager.TicksGame;

        PawnRankData data = comp?.GetRankData(pawn);
        if (data != null)
            data.lastStudiedTick = now;

        PawnRankData partnerData = comp?.GetRankData(Partner);
        if (partnerData != null)
            partnerData.lastStudiedTick = now;

        Messages.Message(RankTrackUtility.Text(data?.track, "ESCP_Rank_StudyingComplete", pawn.LabelShortCap, Partner.LabelShortCap), pawn, MessageTypeDefOf.PositiveEvent);
        pawn.needs?.mood?.thoughts.memories.TryGainMemory(RankThoughtDefOf.ESCP_Thought_RankStudied);
        Partner.needs?.mood?.thoughts.memories.TryGainMemory(RankThoughtDefOf.ESCP_Thought_RankStudied);
    }
}
