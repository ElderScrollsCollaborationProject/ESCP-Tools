using ESCP.Enlist;
using HarmonyLib;

namespace ESCP.Ranks;

[HarmonyPatch(typeof(WorldEnlistTracker), nameof(WorldEnlistTracker.Delist))]
public static class WorldEnlistTracker_Delist_Patch
{
    [HarmonyPostfix]
    public static void Postfix(Faction otherFaction)
    {
        RankTrackUtility.SuspendHediffsForFaction(otherFaction);
    }
}

[HarmonyPatch(typeof(WorldEnlistTracker), nameof(WorldEnlistTracker.KickOut))]
public static class WorldEnlistTracker_KickOut_Patch
{
    [HarmonyPostfix]
    public static void Postfix(Faction otherFaction)
    {
        RankTrackUtility.SuspendHediffsForFaction(otherFaction);
        RankTrackUtility.DemoteAllForFaction(otherFaction, "ESCP_Rank_Demoted_KickedOut");
    }
}
