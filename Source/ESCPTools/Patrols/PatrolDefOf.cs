using ESCP.Core;
using Verse.AI;

namespace ESCP.Patrols;

[DefOf]
public static class PatrolDefOf
{
    public static WorldObjectDef ESCP_Patrol;
    public static DutyDef ESCP_Duty_Patrol;
    public static ModOptionDef ESCP_Option_FactionPatrols;

    static PatrolDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(PatrolDefOf));
    }
}
