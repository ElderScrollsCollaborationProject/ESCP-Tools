using RimWorld.Planet;
using UnityEngine;
using Verse.AI.Group;

namespace ESCP.Patrols;

public static class PatrolUtility
{
    public static float Points => Mathf.Clamp(StorytellerUtility.DefaultThreatPointsNow(Find.World) * 0.5f, 150f, 900f);

    public static bool CanPatrol(Faction faction)
    {
        return !faction.IsPlayer && !faction.defeated && !faction.Hidden && faction.def.humanlikeFaction
            && faction.def.defName.StartsWith("ESCP_") && faction.def.pawnGroupMakers != null
            && faction.def.pawnGroupMakers.Any(g => g.kindDef == PawnGroupKindDefOf.Combat);
    }

    public static string Label(Faction faction) => "ESCP_Patrol_Label".Translate(faction.Name);

    public static List<Pawn> Generate(Faction faction, float points, PlanetTile tile)
    {
        return PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
        {
            groupKind = PawnGroupKindDefOf.Combat,
            faction = faction,
            points = points,
            tile = tile,
            generateFightersOnly = true
        }).ToList();
    }

    public static bool TryLaunch(Faction faction)
    {
        List<Settlement> own = Find.WorldObjects.Settlements.Where(s => s.Faction == faction).ToList();
        if (own.Count == 0)
            return false;

        Settlement from = own.RandomElement();
        PlanetTile dest = PlanetTile.Invalid;
        Map home = Find.AnyPlayerHomeMap;
        float homeChance = faction.HostileTo(Faction.OfPlayer) ? 0.04f : 0.08f;
        if (home != null && Rand.Chance(homeChance) && Find.WorldGrid.ApproxDistanceInTiles(from.Tile, home.Tile) < 25f)
            dest = home.Tile;
        else if (own.Where(s => s != from && Find.WorldGrid.ApproxDistanceInTiles(from.Tile, s.Tile) < 60f).TryRandomElement(out Settlement to))
            dest = to.Tile;
        if (!dest.Valid)
            return false;

        WorldObject_Patrol patrol = (WorldObject_Patrol)WorldObjectMaker.MakeWorldObject(PatrolDefOf.ESCP_Patrol);
        patrol.Tile = from.Tile;
        patrol.SetFaction(faction);
        patrol.Setup(dest, Points);
        Find.WorldObjects.Add(patrol);
        return true;
    }

    public static bool SpawnOnMap(Faction faction, Map map, float points)
    {
        if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 entry, map, CellFinder.EdgeRoadChance_Hostile)
            || !RCellFinder.TryFindTravelDestFrom(entry, map, out IntVec3 dest))
            return false;

        List<Pawn> pawns = Generate(faction, points, map.Tile);
        if (pawns.Count == 0)
            return false;

        foreach (Pawn pawn in pawns)
            GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(entry, map, 5), map);
        LordMaker.MakeNewLord(faction, new LordJob_Patrol(dest), map, pawns);

        if (faction.HostileTo(Faction.OfPlayer))
            Find.LetterStack.ReceiveLetter("ESCP_Patrol_MapHostileTitle".Translate(), "ESCP_Patrol_MapHostile".Translate(faction.NameColored), LetterDefOf.ThreatSmall, pawns[0]);
        else
            Messages.Message("ESCP_Patrol_MapPeaceful".Translate(faction.NameColored), pawns[0], MessageTypeDefOf.NeutralEvent);
        return true;
    }

    public static bool Encounter(Caravan caravan, Faction faction, float points)
    {
        if (!faction.HostileTo(Faction.OfPlayer))
        {
            Messages.Message("ESCP_Patrol_RoadPeaceful".Translate(caravan.Label, faction.NameColored), caravan, MessageTypeDefOf.NeutralEvent);
            return false;
        }

        if (!CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(caravan.Tile))
            return false;

        List<Pawn> pawns = Generate(faction, points, caravan.Tile);
        if (pawns.Count == 0)
            return false;

        Map map = CaravanIncidentUtility.SetupCaravanAttackMap(caravan, pawns, true);
        LordMaker.MakeNewLord(faction, new LordJob_AssaultColony(faction, true, true, false, false, false), map, pawns);
        Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
        Find.LetterStack.ReceiveLetter("ESCP_Patrol_RoadHostileTitle".Translate(), "ESCP_Patrol_RoadHostile".Translate(caravan.Label, faction.NameColored), LetterDefOf.ThreatBig, pawns[0]);
        return true;
    }
}
