using HarmonyLib;
using Verse.AI;

namespace ESCP.Core.Vampires;

public class HediffCompProperties_TurnVampire : HediffCompProperties
{
    public HediffCompProperties_TurnVampire()
    {
        compClass = typeof(HediffComp_TurnVampire);
    }
}

public class HediffComp_TurnVampire : HediffComp
{
    public override void CompPostTickInterval(ref float severityAdjustment, int delta)
    {
        if (parent.Severity < parent.def.maxSeverity)
            return;

        VampireUtility.Turn(Pawn);
    }
}

public class Hediff_Vampirism : HediffWithComps
{
    private const int SunCheckInterval = 250;
    private const float SunGlow = 0.5f;
    private const float BurnSeverity = 0.75f;

    public override void PostAdd(DamageInfo? dinfo)
    {
        base.PostAdd(dinfo);
        VampireUtility.GrantPowers(pawn);
    }

    public override void PostRemoved()
    {
        base.PostRemoved();
        if (pawn.abilities == null)
            return;
        pawn.abilities.RemoveAbility(VampireDefOf.ESCP_Ability_VampireFeed);
        pawn.abilities.RemoveAbility(VampireDefOf.ESCP_Ability_VampiricDrain);
        pawn.abilities.RemoveAbility(VampireDefOf.ESCP_Ability_VampireTurn);
    }

    public override void PostTickInterval(int delta)
    {
        base.PostTickInterval(delta);
        if (!pawn.IsHashIntervalTick(SunCheckInterval, delta))
            return;

        VampireUtility.GrantPowers(pawn);
        if (!pawn.Spawned || pawn.Position.Roofed(pawn.Map) || pawn.Map.skyManager.CurSkyGlow < SunGlow)
            return;

        HealthUtility.AdjustSeverity(pawn, VampireDefOf.ESCP_Hediff_Sunlight, 0.1f);
        if (Severity >= BurnSeverity)
            pawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, 1f));
    }
}

public class CompProperties_AbilityFeed : CompProperties_AbilityEffect
{
    public float bloodLoss = 0.25f;
    public float infectChance = 0.2f;

    public CompProperties_AbilityFeed()
    {
        compClass = typeof(CompAbilityEffect_Feed);
    }
}

public class CompAbilityEffect_Feed : CompAbilityEffect
{
    public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
    {
        Pawn victim = target.Pawn;
        if (victim != null && victim.RaceProps.Humanlike && (victim.Downed || !victim.Awake() || victim.IsPrisoner) && !VampireUtility.IsVampire(victim))
            return base.Valid(target, throwMessages);

        if (throwMessages)
            Messages.Message("ESCP_Vampire_CannotFeed".Translate(), MessageTypeDefOf.RejectInput, false);
        return false;
    }

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        Pawn victim = target.Pawn;
        CompProperties_AbilityFeed feed = (CompProperties_AbilityFeed)props;

        HealthUtility.AdjustSeverity(victim, HediffDefOf.BloodLoss, feed.bloodLoss);
        VampireUtility.Sate(parent.pawn);
        if (Rand.Chance(feed.infectChance))
            VampireUtility.TryInfect(victim);
        victim.needs?.mood?.thoughts?.memories?.TryGainMemory(VampireDefOf.ESCP_Thought_FedUpon, parent.pawn);
    }
}

public class CompProperties_AbilityDrain : CompProperties_AbilityEffect
{
    public float damage = 12f;

    public CompProperties_AbilityDrain()
    {
        compClass = typeof(CompAbilityEffect_Drain);
    }
}

public class CompAbilityEffect_Drain : CompAbilityEffect
{
    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        if (target.Pawn == null)
            return;

        float damage = ((CompProperties_AbilityDrain)props).damage;
        target.Pawn.TakeDamage(new DamageInfo(DamageDefOf.Burn, damage, 0f, -1f, parent.pawn));
        Magic.MagickaUtility.HealInjuries(parent.pawn, damage);
    }
}

public class CompProperties_AbilityTurn : CompProperties_AbilityEffect
{
    public CompProperties_AbilityTurn()
    {
        compClass = typeof(CompAbilityEffect_Turn);
    }
}

public class CompAbilityEffect_Turn : CompAbilityEffect
{
    public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
    {
        if (target.Pawn != null && VampireUtility.CanPrey(target.Pawn))
            return base.Valid(target, throwMessages);

        if (throwMessages)
            Messages.Message("ESCP_Vampire_CannotTurn".Translate(), MessageTypeDefOf.RejectInput, false);
        return false;
    }

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        VampireUtility.Sate(parent.pawn);
        VampireUtility.Turn(target.Pawn);
    }
}

public class MentalStateExtension_BloodFrenzy : DefModExtension
{
    public float turnChance = 0.5f;
    public int feedTicks = 240;
}

public class MentalBreakWorker_BloodFrenzy : MentalBreakWorker
{
    public override bool BreakCanOccur(Pawn pawn)
    {
        return VampireUtility.IsVampire(pawn) && VampireUtility.FindSleepingVictim(pawn) != null && base.BreakCanOccur(pawn);
    }
}

public class JobGiver_BloodFrenzy : ThinkNode_JobGiver
{
    protected override Job TryGiveJob(Pawn pawn)
    {
        Pawn victim = VampireUtility.FindSleepingVictim(pawn);
        if (victim == null)
        {
            pawn.MentalState?.RecoverFromState();
            return null;
        }
        return JobMaker.MakeJob(VampireDefOf.ESCP_Job_BloodFrenzy, victim);
    }
}

public class JobDriver_BloodFrenzy : JobDriver
{
    private Pawn Victim => job.targetA.Pawn;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Victim, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        MentalStateExtension_BloodFrenzy frenzy = pawn.MentalStateDef?.GetModExtension<MentalStateExtension_BloodFrenzy>();
        if (frenzy == null)
            yield break;

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => Victim.Awake());
        yield return Toils_General.Wait(frenzy.feedTicks, TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A).FailOn(() => Victim.Awake());
        yield return new Toil
        {
            initAction = () =>
            {
                VampireUtility.Sate(pawn);
                if (Rand.Chance(frenzy.turnChance))
                    VampireUtility.Turn(Victim);
                else
                    VampireUtility.DrainToDeath(Victim, pawn);
                pawn.MentalState?.RecoverFromState();
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}

public class JobExtension_VampireFeed : DefModExtension
{
    public float thirstToFeed = 0.25f;
    public float bloodLoss = 0.2f;
    public int feedTicks = 240;
}

public class JobGiver_VampireFeed : ThinkNode_JobGiver
{
    private const float FeedPriority = 9.2f;

    private static JobExtension_VampireFeed Settings => VampireDefOf.ESCP_Job_VampireFeed.GetModExtension<JobExtension_VampireFeed>();

    public override float GetPriority(Pawn pawn)
    {
        return VampireUtility.Thirst(pawn) >= Settings.thirstToFeed ? FeedPriority : 0f;
    }

    protected override Job TryGiveJob(Pawn pawn)
    {
        JobExtension_VampireFeed settings = Settings;
        if (VampireUtility.Thirst(pawn) < settings.thirstToFeed || pawn.Faction != Faction.OfPlayer || pawn.IsPrisoner)
            return null;

        Pawn closest = null;
        int closestDist = int.MaxValue;
        foreach (Pawn prisoner in pawn.Map.mapPawns.PrisonersOfColonySpawned)
        {
            if (!prisoner.guest.IsInteractionEnabled(VampireDefOf.ESCP_PrisonerMode_VampireFeed) || VampireUtility.IsVampire(prisoner) || SanguophageUtility.WouldDieFromAdditionalBloodLoss(prisoner, settings.bloodLoss))
                continue;
            int dist = pawn.Position.DistanceToSquared(prisoner.Position);
            if (dist < closestDist && pawn.CanReserveAndReach(prisoner, PathEndMode.Touch, Danger.Some))
            {
                closest = prisoner;
                closestDist = dist;
            }
        }
        return closest == null ? null : JobMaker.MakeJob(VampireDefOf.ESCP_Job_VampireFeed, closest);
    }
}

public class JobDriver_VampireFeed : JobDriver
{
    private Pawn Prisoner => job.targetA.Pawn;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Prisoner, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        JobExtension_VampireFeed settings = job.def.GetModExtension<JobExtension_VampireFeed>();

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_General.Wait(settings.feedTicks, TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A);
        yield return new Toil
        {
            initAction = () =>
            {
                HealthUtility.AdjustSeverity(Prisoner, HediffDefOf.BloodLoss, settings.bloodLoss);
                VampireUtility.Sate(pawn);
                Prisoner.needs?.mood?.thoughts?.memories?.TryGainMemory(VampireDefOf.ESCP_Thought_FedUpon, pawn);
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}

public class PawnKindExtension_Vampire : DefModExtension
{
}

public static class VampireUtility
{
    private const float BiteChance = 0.05f;

    public static bool IsVampire(Pawn pawn)
    {
        return pawn.health.hediffSet.HasHediff(VampireDefOf.ESCP_Hediff_Vampirism);
    }

    public static void GrantPowers(Pawn pawn)
    {
        pawn.abilities?.GainAbility(VampireDefOf.ESCP_Ability_VampireFeed);
        pawn.abilities?.GainAbility(VampireDefOf.ESCP_Ability_VampiricDrain);
        pawn.abilities?.GainAbility(VampireDefOf.ESCP_Ability_VampireTurn);
    }

    public static float Thirst(Pawn pawn)
    {
        return pawn.health.hediffSet.GetFirstHediffOfDef(VampireDefOf.ESCP_Hediff_Vampirism)?.Severity ?? 0f;
    }

    public static bool CanPrey(Pawn pawn)
    {
        return pawn.RaceProps.Humanlike && pawn.RaceProps.IsFlesh && !pawn.Dead && !pawn.Awake() && !IsVampire(pawn);
    }

    public static Pawn FindSleepingVictim(Pawn vampire)
    {
        if (!vampire.Spawned)
            return null;

        Pawn closest = null;
        int closestDist = int.MaxValue;
        foreach (Pawn other in vampire.Map.mapPawns.AllHumanlikeSpawned)
        {
            if (other == vampire || !CanPrey(other))
                continue;
            int dist = vampire.Position.DistanceToSquared(other.Position);
            if (dist < closestDist && vampire.CanReserveAndReach(other, PathEndMode.Touch, Danger.Deadly))
            {
                closest = other;
                closestDist = dist;
            }
        }
        return closest;
    }

    public static void Turn(Pawn pawn)
    {
        Hediff disease = pawn.health.hediffSet.GetFirstHediffOfDef(VampireDefOf.ESCP_Hediff_SanguinareVampiris);
        if (disease != null)
            pawn.health.RemoveHediff(disease);
        pawn.health.AddHediff(VampireDefOf.ESCP_Hediff_Vampirism);
        if (pawn.Faction == Faction.OfPlayer)
            Find.LetterStack.ReceiveLetter("ESCP_Vampire_TurnedLabel".Translate(pawn.LabelShort), "ESCP_Vampire_TurnedText".Translate(pawn.LabelShort), LetterDefOf.NegativeEvent, pawn);
    }

    public static void DrainToDeath(Pawn victim, Pawn vampire)
    {
        HealthUtility.AdjustSeverity(victim, HediffDefOf.BloodLoss, 1f);
        if (!victim.Dead)
            victim.Kill(null, victim.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss));
        if (victim.Faction == Faction.OfPlayer || vampire.Faction == Faction.OfPlayer)
            Messages.Message("ESCP_Vampire_DrainedDead".Translate(vampire.LabelShort, victim.LabelShort), vampire, MessageTypeDefOf.NegativeHealthEvent);
    }

    public static void Sate(Pawn pawn)
    {
        Hediff vampirism = pawn.health.hediffSet.GetFirstHediffOfDef(VampireDefOf.ESCP_Hediff_Vampirism);
        if (vampirism != null)
            vampirism.Severity = vampirism.def.minSeverity;
    }

    public static void TryInfect(Pawn pawn)
    {
        if (pawn.RaceProps.Humanlike && pawn.RaceProps.IsFlesh && !IsVampire(pawn) && !pawn.health.hediffSet.HasHediff(VampireDefOf.ESCP_Hediff_SanguinareVampiris))
            pawn.health.AddHediff(VampireDefOf.ESCP_Hediff_SanguinareVampiris);
    }

    public static void Notify_MeleeHit(Pawn attacker, Pawn victim)
    {
        if (Rand.Chance(BiteChance) && IsVampire(attacker))
            TryInfect(victim);
    }
}

[HarmonyPatch(typeof(DamageWorker_AddInjury), "ApplyToPawn")]
public static class DamageWorker_VampireBite_Patch
{
    public static void Postfix(DamageInfo dinfo, Pawn pawn)
    {
        if (dinfo.Instigator is Pawn attacker && attacker != pawn && !pawn.Dead && dinfo.Def.isRanged == false && dinfo.Def.harmsHealth && attacker.RaceProps.Humanlike)
            VampireUtility.Notify_MeleeHit(attacker, pawn);
    }
}

[HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) })]
public static class PawnGenerator_Vampire_Patch
{
    public static void Postfix(Pawn __result)
    {
        if (__result?.kindDef?.GetModExtension<PawnKindExtension_Vampire>() != null)
            __result.health.AddHediff(VampireDefOf.ESCP_Hediff_Vampirism);
    }
}

public class IncidentWorker_VampireAmbush : IncidentWorker_RaidEnemy
{
    private const float MaxGlow = 0.4f;

    private static Faction Coven => Find.FactionManager.FirstFactionOfDef(VampireDefOf.ESCP_Faction_Volkihar);

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return Coven != null && parms.target is Map map && map.skyManager.CurSkyGlow <= MaxGlow && base.CanFireNowSub(parms);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        parms.faction = Coven;
        parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
        parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
        return base.TryExecuteWorker(parms);
    }
}

[DefOf]
public static class VampireDefOf
{
    [MayRequire("Rick.ESCP.Vampires")]
    public static HediffDef ESCP_Hediff_SanguinareVampiris;
    [MayRequire("Rick.ESCP.Vampires")]
    public static HediffDef ESCP_Hediff_Vampirism;
    [MayRequire("Rick.ESCP.Vampires")]
    public static HediffDef ESCP_Hediff_Sunlight;
    [MayRequire("Rick.ESCP.Vampires")]
    public static AbilityDef ESCP_Ability_VampireFeed;
    [MayRequire("Rick.ESCP.Vampires")]
    public static AbilityDef ESCP_Ability_VampiricDrain;
    [MayRequire("Rick.ESCP.Vampires")]
    public static AbilityDef ESCP_Ability_VampireTurn;
    [MayRequire("Rick.ESCP.Vampires")]
    public static JobDef ESCP_Job_BloodFrenzy;
    [MayRequire("Rick.ESCP.Vampires")]
    public static JobDef ESCP_Job_VampireFeed;
    [MayRequire("Rick.ESCP.Vampires")]
    public static PrisonerInteractionModeDef ESCP_PrisonerMode_VampireFeed;
    [MayRequire("Rick.ESCP.Vampires")]
    public static ThoughtDef ESCP_Thought_FedUpon;
    [MayRequire("Rick.ESCP.Vampires")]
    public static FactionDef ESCP_Faction_Volkihar;

    static VampireDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(VampireDefOf));
    }
}
