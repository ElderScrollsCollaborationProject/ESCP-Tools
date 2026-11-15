namespace ESCP.Ranks.Jobs;

public class JobDriver_RankInspection : JobDriver_RankSession
{
    private const float CommendThreshold = 0.85f;
    private const float ReprimandThreshold = 0.5f;

    protected override int SessionDuration => 1250;

    protected override JobDef PartnerJobDef => RankJobDefOf.ESCP_Job_RankInspectionPartner;

    protected override void TickAction()
    {
    }

    protected override void Complete()
    {
        GameComponent_PawnRanks comp = RankComp;
        PawnRankData inspectorData = comp?.GetRankData(pawn);
        PawnRankData targetData = comp?.GetRankData(Partner);

        if (targetData != null)
            targetData.lastInspectedTick = Find.TickManager.TicksGame;

        float? condition = GetGearCondition(Partner);
        if (condition == null)
        {
            Messages.Message(RankTrackUtility.Text(inspectorData?.track, "ESCP_Rank_InspectionComplete_NoGear", pawn.LabelShortCap, Partner.LabelShortCap), pawn, MessageTypeDefOf.NeutralEvent);
            return;
        }

        if (condition.Value >= CommendThreshold)
            ApplyResult(RankHediffDefOf.ESCP_Hediff_Inspection_Commended, RankThoughtDefOf.ESCP_Thought_Inspection_Commended, "ESCP_Rank_InspectionComplete_Commended", MessageTypeDefOf.PositiveEvent);
        else if (condition.Value < ReprimandThreshold)
            ApplyResult(RankHediffDefOf.ESCP_Hediff_Inspection_Reprimanded, RankThoughtDefOf.ESCP_Thought_Inspection_Reprimanded, "ESCP_Rank_InspectionComplete_Reprimanded", MessageTypeDefOf.NegativeEvent);
        else
            Messages.Message(RankTrackUtility.Text(inspectorData?.track, "ESCP_Rank_InspectionComplete_Passed", pawn.LabelShortCap, Partner.LabelShortCap), pawn, MessageTypeDefOf.NeutralEvent);

        inspectorData?.AddHistory(RankTrackUtility.Text(inspectorData.track, "ESCP_Rank_History_Inspected", Partner.LabelShortCap));
    }

    private void ApplyResult(HediffDef hediff, ThoughtDef thought, string messageKey, MessageTypeDef msgType)
    {
        if (Partner.health != null && hediff != null)
        {
            Hediff existing = Partner.health.hediffSet.hediffs.FirstOrDefault(h => h.def == hediff);
            if (existing != null)
                Partner.health.RemoveHediff(existing);
            Partner.health.AddHediff(hediff);
        }

        Partner.needs?.mood?.thoughts.memories.TryGainMemory(thought);
        Messages.Message(RankTrackUtility.Text(RankComp?.GetRankData(pawn)?.track, messageKey, pawn.LabelShortCap, Partner.LabelShortCap), Partner, msgType);
    }

    private static float? GetGearCondition(Pawn target)
    {
        List<float> ratios = new List<float>();

        ThingWithComps weapon = target.equipment?.Primary;
        if (weapon != null && weapon.MaxHitPoints > 0)
            ratios.Add(weapon.HitPoints / (float)weapon.MaxHitPoints);

        if (target.apparel != null)
        {
            foreach (Apparel apparel in target.apparel.WornApparel)
            {
                if (apparel.MaxHitPoints > 0)
                    ratios.Add(apparel.HitPoints / (float)apparel.MaxHitPoints);
            }
        }

        return ratios.Count == 0 ? (float?)null : ratios.Average();
    }
}
