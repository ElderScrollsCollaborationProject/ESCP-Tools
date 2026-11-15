using ESCP.Enlist;
using Verse.AI;
using Verse.AI.Group;

namespace ESCP.Factions;

public class BlackSacramentExtension : DefModExtension
{
    public FactionDef faction;
    public PawnKindDef assassinKind;
    public MentalStateDef mentalState;
}

public class MentalState_Contract : MentalState_MurderousRage
{
    public override bool ForceHostileTo(Thing t) => t.Faction == Faction.OfPlayer;

    public override bool ForceHostileTo(Faction f) => f == Faction.OfPlayer;
}

public class IncidentWorker_BlackSacrament : IncidentWorker
{
    private BlackSacramentExtension Props => def.GetModExtension<BlackSacramentExtension>();

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        Map map = (Map)parms.target;
        return Brotherhood() != null && TryFindVictim(map, out _) && TryFindEntry(map, out _);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Map map = (Map)parms.target;
        Faction brotherhood = Brotherhood();
        if (brotherhood == null || !TryFindVictim(map, out Pawn victim) || !TryFindEntry(map, out IntVec3 entry))
            return false;

        Pawn assassin = PawnGenerator.GeneratePawn(new PawnGenerationRequest(Props.assassinKind, brotherhood, mustBeCapableOfViolence: true));
        GenSpawn.Spawn(assassin, entry, map);
        assassin.guilt.Notify_Guilty();
        LordMaker.MakeNewLord(brotherhood, new LordJob_ExitMapBest(LocomotionUrgency.Jog), map, new[] { assassin });
        assassin.mindState.mentalStateHandler.TryStartMentalState(Props.mentalState, forced: true, transitionSilently: true);
        if (assassin.MentalState is MentalState_MurderousRage rage)
            rage.target = victim;
        return true;
    }

    private Faction Brotherhood()
    {
        Faction faction = Find.FactionManager.FirstFactionOfDef(Props.faction);
        if (faction == null || faction.defeated || faction.PlayerRelationKind == FactionRelationKind.Ally)
            return null;
        foreach (FactionEnlistOptionsDef options in faction.GetEnlistOptions())
        {
            if (WorldEnlistTracker.Instance.EnlistedTo(faction, options))
                return null;
        }
        return faction;
    }

    private static bool TryFindVictim(Map map, out Pawn victim)
    {
        return map.mapPawns.FreeColonistsSpawned.Where(p => !p.Downed && !p.IsQuestLodger()).TryRandomElement(out victim);
    }

    private static bool TryFindEntry(Map map, out IntVec3 entry)
    {
        return CellFinder.TryFindRandomEdgeCellWith(c => c.Standable(map) && !c.Fogged(map) && map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Neutral, out entry);
    }
}
