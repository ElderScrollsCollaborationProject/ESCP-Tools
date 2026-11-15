namespace ESCP.Ranks.Jobs;

[DefOf]
public static class RankJobDefOf
{
    public static JobDef ESCP_Job_RankTraining;
    public static JobDef ESCP_Job_RankDrilling;
    public static JobDef ESCP_Job_RankDrillingPartner;
    public static JobDef ESCP_Job_RankStudying;
    public static JobDef ESCP_Job_RankStudyingPartner;
    public static JobDef ESCP_Job_VisitMentor;
    public static JobDef ESCP_Job_RankInspection;
    public static JobDef ESCP_Job_RankInspectionPartner;
    public static JobDef ESCP_Job_PunishmentDetail;
    public static JobDef ESCP_Job_FormUp;

    static RankJobDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(RankJobDefOf));
    }
}
