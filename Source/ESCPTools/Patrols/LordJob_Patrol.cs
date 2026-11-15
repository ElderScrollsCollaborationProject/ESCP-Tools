using Verse.AI;
using Verse.AI.Group;

namespace ESCP.Patrols;

public class LordJob_Patrol : LordJob
{
    private IntVec3 dest;

    public LordJob_Patrol()
    {
    }

    public LordJob_Patrol(IntVec3 dest)
    {
        this.dest = dest;
    }

    public override StateGraph CreateGraph()
    {
        StateGraph graph = new StateGraph();
        graph.AddToil(new LordToil_Patrol(dest));
        return graph;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref dest, "dest");
    }
}

public class LordToil_Patrol : LordToil
{
    private readonly IntVec3 dest;

    public LordToil_Patrol(IntVec3 dest)
    {
        this.dest = dest;
    }

    public override bool AllowSatisfyLongNeeds => false;

    public override void UpdateAllDuties()
    {
        foreach (Pawn pawn in lord.ownedPawns)
            pawn.mindState.duty = new PawnDuty(PatrolDefOf.ESCP_Duty_Patrol, dest);
    }
}
