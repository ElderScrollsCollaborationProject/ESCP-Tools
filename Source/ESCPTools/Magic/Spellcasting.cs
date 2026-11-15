using HarmonyLib;
using Verse.AI;
using Verse.AI.Group;

namespace ESCP.Core.Magic;

public enum SpellRole
{
    Attack,
    SelfBuff,
    HealAlly,
    HealSelf,
    Summon
}

public class AbilityExtension_SpellAI : DefModExtension
{
    public SpellRole role;
    public HediffDef buffHediff;
    public float healBelow = 0.6f;
}

public class PawnKindExtension_Spellcaster : DefModExtension
{
    public List<AbilityDef> spells;
    public List<AbilityDef> randomSpells;
    public IntRange randomCount = IntRange.Zero;
}

public static class SpellcastingUtility
{
    public static void GrantSpells(Pawn pawn, PawnKindExtension_Spellcaster extension)
    {
        if (pawn.abilities == null)
            return;

        if (extension.spells != null)
        {
            for (int i = 0; i < extension.spells.Count; i++)
                pawn.abilities.GainAbility(extension.spells[i]);
        }

        if (extension.randomSpells != null)
        {
            foreach (AbilityDef spell in extension.randomSpells.TakeRandom(extension.randomCount.RandomInRange))
                pawn.abilities.GainAbility(spell);
        }

        MagickaUtility.For(pawn);
    }

    public static Job SpellJob(Pawn pawn, Thing enemy = null)
    {
        List<Ability> abilities = pawn.abilities?.abilities;
        if (abilities.NullOrEmpty() || pawn.Faction == Faction.OfPlayerSilentFail)
            return null;

        int start = Rand.Range(0, abilities.Count);
        for (int i = 0; i < abilities.Count; i++)
        {
            Ability ability = abilities[(start + i) % abilities.Count];
            AbilityExtension_SpellAI extension = ability.def.GetModExtension<AbilityExtension_SpellAI>();
            if (extension == null || !ability.CanCast || (ability.def.casterMustBeCapableOfViolence && pawn.WorkTagIsDisabled(WorkTags.Violent)))
                continue;

            LocalTargetInfo target = TargetFor(pawn, ability, extension, enemy ?? pawn.mindState.enemyTarget);
            if (target.IsValid && ability.AICanTargetNow(target))
                return ability.GetJob(target, target);
        }
        return null;
    }

    private static LocalTargetInfo TargetFor(Pawn pawn, Ability ability, AbilityExtension_SpellAI extension, Thing enemy)
    {
        switch (extension.role)
        {
            case SpellRole.SelfBuff:
                return extension.buffHediff == null || !pawn.health.hediffSet.HasHediff(extension.buffHediff) ? pawn : LocalTargetInfo.Invalid;
            case SpellRole.HealSelf:
                return pawn.health.summaryHealth.SummaryHealthPercent < extension.healBelow ? pawn : LocalTargetInfo.Invalid;
            case SpellRole.HealAlly:
                return WoundedAlly(pawn, ability, extension);
            default:
                return enemy != null && ability.verb.CanHitTarget(enemy) && (ability.verb as Verb_CastAbility)?.IsApplicableTo(enemy) != false ? enemy : LocalTargetInfo.Invalid;
        }
    }

    private static LocalTargetInfo WoundedAlly(Pawn pawn, Ability ability, AbilityExtension_SpellAI extension)
    {
        List<Pawn> allies = pawn.GetLord()?.ownedPawns;
        if (allies == null)
            return pawn.health.summaryHealth.SummaryHealthPercent < extension.healBelow ? pawn : LocalTargetInfo.Invalid;

        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i].Spawned && !allies[i].Dead && allies[i].health.summaryHealth.SummaryHealthPercent < extension.healBelow && ability.verb.CanHitTarget(allies[i]))
                return allies[i];
        }
        return LocalTargetInfo.Invalid;
    }
}

[HarmonyPatch(typeof(JobGiver_AIFightEnemy), "TryGiveJob")]
public static class JobGiver_AIFightEnemy_TryGiveJob_Patch
{
    public static void Postfix(Pawn pawn, ref Job __result)
    {
        if (__result != null)
            __result = SpellcastingUtility.SpellJob(pawn) ?? __result;
    }
}

[HarmonyPatch(typeof(JobGiver_Manhunter), "TryGiveJob")]
public static class JobGiver_Manhunter_TryGiveJob_Patch
{
    public static void Postfix(Pawn pawn, ref Job __result)
    {
        if (__result?.targetA.Thing is Pawn victim)
            __result = SpellcastingUtility.SpellJob(pawn, victim) ?? __result;
    }
}

[HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), typeof(PawnGenerationRequest))]
public static class PawnGenerator_GeneratePawn_Spells_Patch
{
    public static void Postfix(Pawn __result)
    {
        PawnKindExtension_Spellcaster extension = __result?.kindDef?.GetModExtension<PawnKindExtension_Spellcaster>();
        if (extension != null)
            SpellcastingUtility.GrantSpells(__result, extension);
    }
}
