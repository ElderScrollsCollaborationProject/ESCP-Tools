using RimWorld.Planet;

namespace ESCP.Patrols;

public class WorldComponent_Patrols : WorldComponent
{
    private const int Interval = GenDate.TicksPerHour;
    private const int MaxPatrols = 12;

    public WorldComponent_Patrols(World world) : base(world)
    {
    }

    public override void WorldComponentTick()
    {
        if (Find.TickManager.TicksGame % Interval != 0 || !Rand.Chance(0.1f) || !PatrolDefOf.ESCP_Option_FactionPatrols.Enabled)
            return;

        if (Find.WorldObjects.AllWorldObjects.Count(o => o is WorldObject_Patrol) >= MaxPatrols)
            return;

        if (Find.FactionManager.AllFactionsListForReading.Where(PatrolUtility.CanPatrol).TryRandomElement(out Faction faction))
            PatrolUtility.TryLaunch(faction);
    }
}
