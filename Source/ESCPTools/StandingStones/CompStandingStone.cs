using Verse.AI;

namespace ESCP.Core.StandingStones;

public class CompProperties_StandingStone : CompProperties
{
    public HediffDef blessing;

    public CompProperties_StandingStone()
    {
        compClass = typeof(CompStandingStone);
    }
}

public class CompStandingStone : ThingComp
{
    public const string BlessingPrefix = "ESCP_Hediff_Stone_";

    public HediffDef Blessing => ((CompProperties_StandingStone)props).blessing;

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        string label = "ESCP_Stone_Touch".Translate(Blessing.label);
        if (!selPawn.RaceProps.Humanlike)
            yield break;

        if (selPawn.health.hediffSet.HasHediff(Blessing))
            yield return new FloatMenuOption(label + ": " + "ESCP_Stone_AlreadyBlessed".Translate(), null);
        else if (!selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
            yield return new FloatMenuOption(label + ": " + "NoPath".Translate(), null);
        else if (!selPawn.CanReserve(parent))
            yield return new FloatMenuOption(label + ": " + "Reserved".Translate(), null);
        else
            yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, () =>
                selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(StandingStoneDefOf.ESCP_Job_TouchStone, parent), JobTag.Misc)), selPawn, parent);
    }

    public void Bless(Pawn pawn)
    {
        List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
        for (int i = hediffs.Count - 1; i >= 0; i--)
        {
            if (hediffs[i].def.defName.StartsWith(BlessingPrefix))
                pawn.health.RemoveHediff(hediffs[i]);
        }

        pawn.health.AddHediff(Blessing);
        FleckMaker.ThrowLightningGlow(parent.DrawPos, parent.Map, 2f);
        Messages.Message("ESCP_Stone_Blessed".Translate(pawn.LabelShort, Blessing.label), pawn, MessageTypeDefOf.PositiveEvent);
    }
}

public class JobDriver_TouchStone : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

        Toil touch = Toils_General.Wait(240, TargetIndex.A);
        touch.WithProgressBarToilDelay(TargetIndex.A);
        yield return touch;

        yield return Toils_General.Do(() => job.targetA.Thing.TryGetComp<CompStandingStone>().Bless(pawn));
    }
}

[DefOf]
public static class StandingStoneDefOf
{
    public static JobDef ESCP_Job_TouchStone;

    static StandingStoneDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(StandingStoneDefOf));
    }
}
