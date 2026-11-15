using Verse.AI;

namespace ESCP.Core.Enchantments;

public class CompProperties_EnchantmentScroll : CompProperties
{
    public EnchantmentDef enchantment;

    public CompProperties_EnchantmentScroll()
    {
        compClass = typeof(CompEnchantmentScroll);
    }
}

public class CompEnchantmentScroll : ThingComp
{
    public EnchantmentDef Enchantment => ((CompProperties_EnchantmentScroll)props).enchantment;

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        string label = "ESCP_Enchantment_Apply".Translate(Enchantment.label);
        CompEnchantable weapon = selPawn.equipment?.Primary?.TryGetComp<CompEnchantable>();
        if (weapon == null)
            yield return new FloatMenuOption(label + ": " + "ESCP_Enchantment_NoWeapon".Translate(), null);
        else if (weapon.enchantment != null)
            yield return new FloatMenuOption(label + ": " + "ESCP_Enchantment_AlreadyEnchanted".Translate(), null);
        else if (!selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
            yield return new FloatMenuOption(label + ": " + "NoPath".Translate(), null);
        else if (!selPawn.CanReserve(parent))
            yield return new FloatMenuOption(label + ": " + "Reserved".Translate(), null);
        else
            yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, () =>
            {
                Job job = JobMaker.MakeJob(EnchantmentDefOf.ESCP_Job_ApplyEnchantment, parent);
                job.count = 1;
                selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }), selPawn, parent);
    }
}

public class JobDriver_ApplyEnchantment : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job, 1, 1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

        Toil channel = Toils_General.Wait(300, TargetIndex.A);
        channel.WithProgressBarToilDelay(TargetIndex.A);
        yield return channel;

        yield return Toils_General.Do(() =>
        {
            CompEnchantable weapon = pawn.equipment?.Primary?.TryGetComp<CompEnchantable>();
            if (weapon == null || weapon.enchantment != null)
                return;

            Thing scroll = job.targetA.Thing;
            weapon.enchantment = scroll.TryGetComp<CompEnchantmentScroll>().Enchantment;
            scroll.SplitOff(1).Destroy();
            Messages.Message("ESCP_Enchantment_Applied".Translate(pawn.LabelShort, weapon.parent.Label), pawn, MessageTypeDefOf.PositiveEvent);
        });
    }
}

[DefOf]
public static class EnchantmentDefOf
{
    public static JobDef ESCP_Job_ApplyEnchantment;

    static EnchantmentDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(EnchantmentDefOf));
    }
}
