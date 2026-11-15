namespace ESCP.Ranks;

[DefOf]
public static class RankHediffDefOf
{
    public static HediffDef ESCP_Hediff_MentorDown_Duty;
    public static HediffDef ESCP_Hediff_MentorWellMaintained;
    public static HediffDef ESCP_Hediff_Inspection_Commended;
    public static HediffDef ESCP_Hediff_Inspection_Reprimanded;

    static RankHediffDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(RankHediffDefOf));
    }
}
