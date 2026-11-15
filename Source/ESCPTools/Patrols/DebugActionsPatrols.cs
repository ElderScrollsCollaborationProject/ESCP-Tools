using LudeonTK;
using RimWorld.Planet;

namespace ESCP.Patrols;

public static class DebugActionsPatrols
{
    [DebugAction("ESCP", "Launch Patrol", allowedGameStates = AllowedGameStates.Playing)]
    private static void LaunchPatrol()
    {
        ShowFactions(faction => PatrolUtility.TryLaunch(faction));
    }

    [DebugAction("ESCP", "Send Patrol to Colony", allowedGameStates = AllowedGameStates.Playing)]
    private static void SendPatrolToColony()
    {
        Map home = Find.AnyPlayerHomeMap;
        if (home == null)
            return;
        ShowFactions(faction =>
        {
            Settlement from = Find.WorldObjects.Settlements.Where(s => s.Faction == faction).OrderBy(s => Find.WorldGrid.ApproxDistanceInTiles(s.Tile, home.Tile)).FirstOrDefault();
            if (from == null)
                return false;
            WorldObject_Patrol patrol = (WorldObject_Patrol)WorldObjectMaker.MakeWorldObject(PatrolDefOf.ESCP_Patrol);
            patrol.Tile = from.Tile;
            patrol.SetFaction(faction);
            patrol.Setup(home.Tile, PatrolUtility.Points);
            Find.WorldObjects.Add(patrol);
            return true;
        });
    }

    private static void ShowFactions(Func<Faction, bool> launch)
    {
        var options = new List<DebugMenuOption>();
        foreach (Faction faction in Find.FactionManager.AllFactionsListForReading.Where(PatrolUtility.CanPatrol).OrderBy(f => f.Name))
        {
            options.Add(new DebugMenuOption(faction.Name, DebugMenuOptionMode.Action, () =>
            {
                if (launch(faction))
                    Messages.Message(PatrolUtility.Label(faction) + " launched.", MessageTypeDefOf.NeutralEvent, false);
                else
                    Messages.Message(faction.Name + " has no route for a patrol.", MessageTypeDefOf.RejectInput, false);
            }));
        }
        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, "Factions"));
    }
}
