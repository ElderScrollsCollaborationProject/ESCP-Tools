using HarmonyLib;

namespace ESCP.Ranks;

[HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
public static class Pawn_HealthTracker_MakeDowned_Patch
{
    [HarmonyPostfix]
    public static void Postfix(Pawn ___pawn)
    {
        if (___pawn == null)
            return;

        GameComponent_PawnRanks comp = Current.Game.GetComponent<GameComponent_PawnRanks>();
        if (comp == null)
            return;

        foreach (KeyValuePair<Pawn, PawnRankData> entry in comp.AllTracked.ToList())
        {
            if (entry.Value.mentor != ___pawn || entry.Key == null || entry.Key.Dead)
                continue;

            Pawn mentee = entry.Key;
            if (mentee.health != null && !mentee.health.hediffSet.HasHediff(RankHediffDefOf.ESCP_Hediff_MentorDown_Duty))
                mentee.health.AddHediff(RankHediffDefOf.ESCP_Hediff_MentorDown_Duty);

            entry.Value.AddHistory(RankTrackUtility.Text(entry.Value.track, "ESCP_Rank_History_MentorDown", ___pawn.LabelShortCap));

            Messages.Message(RankTrackUtility.Text(entry.Value.track, "ESCP_Rank_MentorDown_Message", mentee.LabelShortCap, ___pawn.LabelShortCap), mentee, MessageTypeDefOf.NegativeEvent);
        }
    }
}

[HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
public static class Pawn_Kill_MentorDied_Patch
{
    [HarmonyPostfix]
    public static void Postfix(Pawn __instance)
    {
        if (__instance == null || !__instance.Dead)
            return;

        GameComponent_PawnRanks comp = Current.Game.GetComponent<GameComponent_PawnRanks>();
        if (comp == null)
            return;

        foreach (KeyValuePair<Pawn, PawnRankData> entry in comp.AllTracked.ToList())
        {
            if (entry.Value.mentor != __instance || entry.Key == null || entry.Key.Dead)
                continue;

            entry.Value.mentor = null;
            entry.Value.AddHistory(RankTrackUtility.Text(entry.Value.track, "ESCP_Rank_History_MentorDied", __instance.LabelShortCap));
            entry.Key.needs?.mood?.thoughts.memories.TryGainMemory(RankThoughtDefOf.ESCP_Thought_MentorDied);
        }
    }
}
