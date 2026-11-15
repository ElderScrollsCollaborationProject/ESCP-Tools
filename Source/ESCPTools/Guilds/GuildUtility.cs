namespace ESCP.Guilds;

public static class GuildUtility
{
    public static bool EnsureLeader(Faction faction)
    {
        if (faction.leader != null)
            return true;

        if (faction.TryGenerateNewLeader() && faction.leader != null)
            return true;

        PawnKindDef kind = faction.def.pawnGroupMakers?
            .SelectMany(maker => maker.options)
            .Select(option => option.kind)
            .Where(k => k.RaceProps.Humanlike)
            .OrderByDescending(k => k.combatPower)
            .FirstOrDefault();
        if (kind == null)
            return false;

        Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction, forceGenerateNewPawn: true));
        Find.WorldPawns.PassToWorld(pawn, RimWorld.Planet.PawnDiscardDecideMode.KeepForever);
        faction.leader = pawn;
        return true;
    }
}
