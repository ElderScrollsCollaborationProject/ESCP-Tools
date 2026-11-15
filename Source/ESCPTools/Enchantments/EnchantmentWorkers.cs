namespace ESCP.Core.Enchantments;

public class EnchantmentWorker
{
    public EnchantmentDef def;

    public virtual void Apply(Pawn attacker, Pawn victim, float damage)
    {
    }
}

public class EnchantmentWorker_ExtraDamage : EnchantmentWorker
{
    public override void Apply(Pawn attacker, Pawn victim, float damage)
    {
        victim.TakeDamage(new DamageInfo(def.extraDamage, def.magnitude, 0f, -1f, attacker));
    }
}

public class EnchantmentWorker_Hediff : EnchantmentWorker
{
    public override void Apply(Pawn attacker, Pawn victim, float damage)
    {
        HealthUtility.AdjustSeverity(victim, def.hediff, def.magnitude);
    }
}

public class EnchantmentWorker_Stun : EnchantmentWorker
{
    public override void Apply(Pawn attacker, Pawn victim, float damage)
    {
        victim.stances?.stunner?.StunFor((int)def.magnitude, attacker);
    }
}

public class EnchantmentWorker_AbsorbHealth : EnchantmentWorker
{
    public override void Apply(Pawn attacker, Pawn victim, float damage)
    {
        Magic.MagickaUtility.HealInjuries(attacker, damage * def.magnitude);
    }
}
