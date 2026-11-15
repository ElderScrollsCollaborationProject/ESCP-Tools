namespace ESCP.Ranks;

[DefOf]
public static class RankThoughtDefOf
{
    public static ThoughtDef ESCP_Thought_RankTrained;
    public static ThoughtDef ESCP_Thought_RankDrilled;
    public static ThoughtDef ESCP_Thought_RankStudied;
    public static ThoughtDef ESCP_Thought_ObservedMentor;
    public static ThoughtDef ESCP_Thought_MentorDied;
    public static ThoughtDef ESCP_Thought_Inspection_Commended;
    public static ThoughtDef ESCP_Thought_Inspection_Reprimanded;
    public static ThoughtDef ESCP_Thought_PunishmentDetail;
    public static ThoughtDef ESCP_Thought_PromotionCeremony;
    public static ThoughtDef ESCP_Thought_WitnessedPromotion;

    static RankThoughtDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(RankThoughtDefOf));
    }
}
