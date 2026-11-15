using Verse;

namespace ESCP_NecromanticThralls
{
    public class HediffComp_KillOnRemove : HediffComp
    {
        public override void CompPostPostRemoved()
        {
            if (!parent.pawn.Dead)
            {
                parent.pawn.Kill(null);
            }
            base.CompPostPostRemoved();
        }
    }
}
