using Verse.AI.Group;

namespace ESCP.Core.Magic;

public class CompProperties_AbilityMagickaCost : CompProperties_AbilityEffect
{
    public float cost;

    public CompProperties_AbilityMagickaCost()
    {
        compClass = typeof(CompAbilityEffect_MagickaCost);
    }
}

public class CompAbilityEffect_MagickaCost : CompAbilityEffect
{
    private const float XpPerMagicka = 4f;

    private float Cost => ((CompProperties_AbilityMagickaCost)props).cost;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        Hediff_Magicka magicka = MagickaUtility.For(parent.pawn);
        magicka.current = Math.Max(0f, magicka.current - Cost);
        parent.pawn.skills?.Learn(SkillDefOf.Intellectual, Cost * XpPerMagicka);
    }

    public override bool GizmoDisabled(out string reason)
    {
        if (MagickaUtility.For(parent.pawn).current < Cost)
        {
            reason = "ESCP_NotEnoughMagicka".Translate(parent.pawn.LabelShort, Cost.ToString("0"));
            return true;
        }
        reason = null;
        return false;
    }

    public override bool AICanTargetNow(LocalTargetInfo target)
    {
        return MagickaUtility.For(parent.pawn).current >= Cost;
    }

    public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
    {
        return "ESCP_MagickaCost".Translate(Cost.ToString("0"));
    }
}

public class CompProperties_AbilityHeal : CompProperties_AbilityEffect
{
    public float amount = 15f;

    public CompProperties_AbilityHeal()
    {
        compClass = typeof(CompAbilityEffect_Heal);
    }
}

public class CompAbilityEffect_Heal : CompAbilityEffect
{
    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        if (target.Pawn != null)
            MagickaUtility.HealInjuries(target.Pawn, ((CompProperties_AbilityHeal)props).amount);
    }
}

public class CompProperties_AbilityCalm : CompProperties_AbilityEffect
{
    public int stunTicks = 300;

    public CompProperties_AbilityCalm()
    {
        compClass = typeof(CompAbilityEffect_Calm);
    }
}

public class CompAbilityEffect_Calm : CompAbilityEffect
{
    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        Pawn pawn = target.Pawn;
        if (pawn == null)
            return;

        if (pawn.InMentalState)
            pawn.MentalState.RecoverFromState();
        pawn.stances?.stunner?.StunFor(((CompProperties_AbilityCalm)props).stunTicks, parent.pawn, addBattleLog: false);
    }
}

public class CompProperties_AbilityParalyze : CompProperties_AbilityEffect
{
    public int stunTicks = 360;

    public CompProperties_AbilityParalyze()
    {
        compClass = typeof(CompAbilityEffect_Paralyze);
    }
}

public class CompAbilityEffect_Paralyze : CompAbilityEffect
{
    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        target.Pawn?.stances?.stunner?.StunFor(((CompProperties_AbilityParalyze)props).stunTicks, parent.pawn);
    }
}

public class CompProperties_AbilityDisarm : CompProperties_AbilityEffect
{
    public CompProperties_AbilityDisarm()
    {
        compClass = typeof(CompAbilityEffect_Disarm);
    }
}

public class CompAbilityEffect_Disarm : CompAbilityEffect
{
    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        Pawn pawn = target.Pawn;
        ThingWithComps primary = pawn?.equipment?.Primary;
        if (primary == null)
            return;

        pawn.equipment.TryDropEquipment(primary, out _, pawn.Position);
        MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "ESCP_Disarmed".Translate(), 3f);
    }

    public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
    {
        Pawn pawn = target.Pawn;
        if (pawn == null)
            return false;
        if (pawn.equipment?.Primary != null)
            return true;

        if (throwMessages)
            Messages.Message("ESCP_DisarmNoWeapon".Translate(pawn.LabelShort), MessageTypeDefOf.RejectInput, historical: false);
        return false;
    }

    public override bool AICanTargetNow(LocalTargetInfo target)
    {
        return target.Pawn?.equipment?.Primary != null;
    }
}

public class CompProperties_AbilityConjure : CompProperties_AbilityEffect
{
    public PawnKindDef pawnKind;
    public int count = 1;

    public CompProperties_AbilityConjure()
    {
        compClass = typeof(CompAbilityEffect_Conjure);
    }
}

public class CompAbilityEffect_Conjure : CompAbilityEffect
{
    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        CompProperties_AbilityConjure conjure = (CompProperties_AbilityConjure)props;
        Map map = parent.pawn.Map;
        List<Pawn> pawns = new List<Pawn>();

        for (int i = 0; i < conjure.count; i++)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(conjure.pawnKind, parent.pawn.Faction, forceGenerateNewPawn: true, canGeneratePawnRelations: false));
            pawn.health.AddHediff(HediffDefOf_Conjured.ESCP_Hediff_Conjured);
            GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(target.Cell, map, 2), map);
            FleckMaker.ThrowLightningGlow(pawn.DrawPos, map, 1.5f);
            pawns.Add(pawn);
        }

        LordMaker.MakeNewLord(parent.pawn.Faction, new LordJob_DefendPoint(target.Cell), map, pawns);
    }
}

public class HediffCompProperties_Conjured : HediffCompProperties
{
    public int lifetimeTicks = 3600;

    public HediffCompProperties_Conjured()
    {
        compClass = typeof(HediffComp_Conjured);
    }
}

public class HediffComp_Conjured : HediffComp
{
    private int age;

    public override void CompPostTickInterval(ref float severityAdjustment, int delta)
    {
        age += delta;
        if (age >= ((HediffCompProperties_Conjured)props).lifetimeTicks && !Pawn.Destroyed)
            Pawn.Destroy();
    }

    public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
    {
        Pawn.Corpse?.Destroy();
    }

    public override void CompExposeData()
    {
        Scribe_Values.Look(ref age, "age");
    }
}

[DefOf]
public static class HediffDefOf_Conjured
{
    public static HediffDef ESCP_Hediff_Conjured;

    static HediffDefOf_Conjured()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf_Conjured));
    }
}

public class SpellProjectileExtension : DefModExtension
{
    public HediffDef hediff;
    public float severity = 0.35f;
}

public class Projectile_SpellHediff : Bullet
{
    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        base.Impact(hitThing, blockedByShield);
        SpellProjectileExtension extension = def.GetModExtension<SpellProjectileExtension>();
        if (hitThing is Pawn { Dead: false } pawn && extension?.hediff != null && !blockedByShield)
            HealthUtility.AdjustSeverity(pawn, extension.hediff, extension.severity);
    }
}
