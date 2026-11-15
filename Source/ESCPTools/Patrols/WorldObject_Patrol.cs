using RimWorld.Planet;
using UnityEngine;

namespace ESCP.Patrols;

public class WorldObject_Patrol : WorldObject
{
    private const int EncounterCooldown = 2 * GenDate.TicksPerDay;

    private int destination = -1;
    private int next = -1;
    private float points;
    private int progress;
    private int costToNext = 1;
    private int lastEncounterTick = -EncounterCooldown;
    private WorldPath path;

    private PlanetTile Destination => new PlanetTile(destination);

    public void Setup(PlanetTile dest, float points)
    {
        destination = dest.tileId;
        this.points = points;
    }

    public override string Label => Faction != null ? PatrolUtility.Label(Faction) : base.Label;

    public override Texture2D ExpandingIcon => Faction?.def.FactionIcon ?? base.ExpandingIcon;

    public override Color ExpandingIconColor => Faction?.Color ?? base.ExpandingIconColor;

    public override Vector3 DrawPos
    {
        get
        {
            Vector3 from = Find.WorldGrid.GetTileCenter(Tile);
            return next < 0 ? from : Vector3.Lerp(from, Find.WorldGrid.GetTileCenter(new PlanetTile(next)), Mathf.Clamp01(progress / (float)costToNext));
        }
    }

    protected override void TickInterval(int delta)
    {
        base.TickInterval(delta);
        if (Faction == null || Faction.defeated || !PatrolDefOf.ESCP_Option_FactionPatrols.Enabled)
        {
            End();
            return;
        }

        if (next < 0 && !StepPath())
        {
            End();
            return;
        }

        progress += delta;
        if (progress < costToNext)
            return;

        progress = 0;
        Tile = new PlanetTile(next);
        next = -1;
        Arrive();
    }

    private bool StepPath()
    {
        if (Tile == Destination)
            return false;

        if (path == null || !path.Found || path.NodesLeftCount == 0)
        {
            path?.ReleaseToPool();
            path = Tile.Layer.Pather.FindPath(Tile, Destination, null, null);
            if (path == null || !path.Found)
                return false;
        }

        PlanetTile step = path.ConsumeNextNode();
        while (step == Tile && path.NodesLeftCount > 0)
            step = path.ConsumeNextNode();
        if (step == Tile)
            return false;

        next = step.tileId;
        costToNext = Mathf.Max(1, Caravan_PathFollower.CostToMove(CaravanTicksPerMoveUtility.DefaultTicksPerMove, Tile, step, null, false, null, null, false));
        return true;
    }

    private void Arrive()
    {
        if (Find.TickManager.TicksGame - lastEncounterTick >= EncounterCooldown)
        {
            foreach (Caravan caravan in Find.WorldObjects.Caravans)
            {
                if (caravan.Tile != Tile || !caravan.IsPlayerControlled)
                    continue;
                lastEncounterTick = Find.TickManager.TicksGame;
                if (PatrolUtility.Encounter(caravan, Faction, points))
                {
                    End();
                    return;
                }
            }
        }

        MapParent parent = Find.WorldObjects.MapParentAt(Tile);
        if (parent?.Map != null && parent.Map.IsPlayerHome)
        {
            PatrolUtility.SpawnOnMap(Faction, parent.Map, points);
            End();
        }
    }

    private void End()
    {
        path?.ReleaseToPool();
        path = null;
        if (!Destroyed)
            Destroy();
    }

    public override string GetInspectString()
    {
        string text = base.GetInspectString();
        MapParent target = Find.WorldObjects.MapParentAt(Destination);
        if (target != null)
        {
            if (!text.NullOrEmpty())
                text += "\n";
            text += "ESCP_Patrol_BoundFor".Translate(target.LabelCap);
        }
        return text;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref destination, "destination", -1);
        Scribe_Values.Look(ref next, "next", -1);
        Scribe_Values.Look(ref points, "points");
        Scribe_Values.Look(ref progress, "progress");
        Scribe_Values.Look(ref costToNext, "costToNext", 1);
        Scribe_Values.Look(ref lastEncounterTick, "lastEncounterTick", -EncounterCooldown);
    }
}
