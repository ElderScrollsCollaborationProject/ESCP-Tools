using Verse.AI;

namespace ESCP.Core.Enchantments;

public class CompProperties_SoulGem : CompProperties
{
    public int tier;
    public bool black;
    public ThingDef filledDef;
    public int charges;

    public CompProperties_SoulGem()
    {
        compClass = typeof(CompSoulGem);
    }
}

public class CompSoulGem : ThingComp
{
    public CompProperties_SoulGem Props => (CompProperties_SoulGem)props;

    public bool IsFilled => Props.filledDef == null;

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        if (!IsFilled)
            yield break;

        string label = "ESCP_SoulGem_Recharge".Translate();
        CompEnchantable weapon = selPawn.equipment?.Primary?.TryGetComp<CompEnchantable>();
        if (weapon?.enchantment == null)
            yield return new FloatMenuOption(label + ": " + "ESCP_SoulGem_NoEnchantedWeapon".Translate(), null);
        else if (weapon.charges >= CompEnchantable.MaxCharges)
            yield return new FloatMenuOption(label + ": " + "ESCP_SoulGem_FullyCharged".Translate(), null);
        else if (!selPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
            yield return new FloatMenuOption(label + ": " + "NoPath".Translate(), null);
        else if (!selPawn.CanReserve(parent))
            yield return new FloatMenuOption(label + ": " + "Reserved".Translate(), null);
        else
            yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, () =>
            {
                Job job = JobMaker.MakeJob(SoulGemDefOf.ESCP_Job_RechargeWeapon, parent);
                job.count = 1;
                selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }), selPawn, parent);
    }
}

public class JobDriver_RechargeWeapon : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job, 1, 1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

        Toil channel = Toils_General.Wait(180, TargetIndex.A);
        channel.WithProgressBarToilDelay(TargetIndex.A);
        yield return channel;

        yield return Toils_General.Do(() =>
        {
            CompEnchantable weapon = pawn.equipment?.Primary?.TryGetComp<CompEnchantable>();
            Thing gem = job.targetA.Thing;
            if (weapon?.enchantment == null)
                return;

            weapon.Recharge(gem.TryGetComp<CompSoulGem>().Props.charges);
            gem.SplitOff(1).Destroy();
            Messages.Message("ESCP_SoulGem_Recharged".Translate(pawn.LabelShort, weapon.parent.Label), pawn, MessageTypeDefOf.PositiveEvent);
        });
    }
}

public class Hediff_SoulTrap : HediffWithComps
{
    public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
    {
        base.Notify_PawnDied(dinfo, culprit);
        SoulGemUtility.TryCapture(pawn);
    }
}

public static class SoulGemUtility
{
    private const int GrandTier = 4;

    public static int TierFor(Pawn pawn)
    {
        float size = pawn.BodySize;
        if (size < 0.5f) return 0;
        if (size < 1f) return 1;
        if (size < 1.5f) return 2;
        if (size < 2.5f) return 3;
        return GrandTier;
    }

    public static void TryCapture(Pawn victim)
    {
        Map map = victim.MapHeld;
        if (map == null || !victim.RaceProps.IsFlesh)
            return;

        Thing gem = FindEmptyGem(victim, map);
        if (gem == null)
        {
            Messages.Message("ESCP_SoulGem_NoGem".Translate(victim.LabelShort), new TargetInfo(victim.PositionHeld, map), MessageTypeDefOf.NeutralEvent);
            return;
        }

        ThingDef filled = gem.TryGetComp<CompSoulGem>().Props.filledDef;
        gem.SplitOff(1).Destroy();
        GenPlace.TryPlaceThing(ThingMaker.MakeThing(filled), victim.PositionHeld, map, ThingPlaceMode.Near);
        Messages.Message("ESCP_SoulGem_Captured".Translate(victim.LabelShort, filled.label), new TargetInfo(victim.PositionHeld, map), MessageTypeDefOf.PositiveEvent);
    }

    private static Thing FindEmptyGem(Pawn victim, Map map)
    {
        bool humanlike = victim.RaceProps.Humanlike;
        int tier = TierFor(victim);
        Thing best = null;
        int bestTier = int.MaxValue;

        foreach (Thing thing in map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver))
        {
            CompSoulGem comp = thing.TryGetComp<CompSoulGem>();
            if (comp == null || comp.IsFilled || comp.Props.black != humanlike || comp.Props.tier < (humanlike ? 0 : tier) || comp.Props.tier >= bestTier)
                continue;
            best = thing;
            bestTier = comp.Props.tier;
        }
        return best;
    }
}

[DefOf]
public static class SoulGemDefOf
{
    public static JobDef ESCP_Job_RechargeWeapon;

    static SoulGemDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(SoulGemDefOf));
    }
}
