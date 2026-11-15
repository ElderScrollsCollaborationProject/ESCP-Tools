using Verse.AI;

namespace ESCP.Core.Magic;

public class CompProperties_SpellTome : CompProperties
{
    public AbilityDef spell;
    public int minIntellectual;

    public CompProperties_SpellTome()
    {
        compClass = typeof(CompSpellTome);
    }
}

public class CompSpellTome : ThingComp
{
    public CompProperties_SpellTome Props => (CompProperties_SpellTome)props;

    public override string CompInspectStringExtra()
    {
        return "ESCP_Tome_Inspect".Translate(Props.spell.label, Props.minIntellectual);
    }

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        string label = "ESCP_Tome_Study".Translate(Props.spell.label);
        if (selPawn.abilities == null || selPawn.skills == null)
            yield break;

        if (selPawn.abilities.GetAbility(Props.spell) != null)
            yield return new FloatMenuOption(label + ": " + "ESCP_Tome_AlreadyKnown".Translate(), null);
        else if (Props.spell.casterMustBeCapableOfViolence && selPawn.WorkTagIsDisabled(WorkTags.Violent))
            yield return new FloatMenuOption(label + ": " + "IsIncapableOfViolenceLower".Translate(selPawn.LabelShort, selPawn), null);
        else if (selPawn.skills.GetSkill(SkillDefOf.Intellectual).TotallyDisabled || selPawn.skills.GetSkill(SkillDefOf.Intellectual).Level < Props.minIntellectual)
            yield return new FloatMenuOption(label + ": " + "ESCP_Tome_TooHard".Translate(Props.minIntellectual), null);
        else if (!selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
            yield return new FloatMenuOption(label + ": " + "NoPath".Translate(), null);
        else if (!selPawn.CanReserve(parent))
            yield return new FloatMenuOption(label + ": " + "Reserved".Translate(), null);
        else
            yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, () =>
                selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(MagicDefOf.ESCP_Job_StudyTome, parent), JobTag.Misc)), selPawn, parent);
    }

    public void TeachTo(Pawn pawn)
    {
        pawn.abilities.GainAbility(Props.spell);
        MagickaUtility.For(pawn);
        Messages.Message("ESCP_Tome_Learned".Translate(pawn.LabelShort, Props.spell.label), pawn, MessageTypeDefOf.PositiveEvent);
        parent.SplitOff(1).Destroy();
    }
}

public class JobDriver_StudyTome : JobDriver
{
    private const int StudyTicks = 2500;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

        Toil study = Toils_General.Wait(StudyTicks, TargetIndex.A);
        study.WithProgressBarToilDelay(TargetIndex.A);
        yield return study;

        yield return Toils_General.Do(() => job.targetA.Thing.TryGetComp<CompSpellTome>().TeachTo(pawn));
    }
}
