using HarmonyLib;

namespace ESCP.Core.Enchantments;

[HarmonyPatch(typeof(DamageWorker_AddInjury), "ApplyToPawn")]
public static class DamageWorker_Enchantment_Patch
{
    private static bool applying;

    public static void Postfix(DamageInfo dinfo, Pawn pawn)
    {
        if (applying || dinfo.Instigator is not Pawn attacker || attacker == pawn || pawn.Dead)
            return;

        ThingWithComps weapon = attacker.equipment?.Primary;
        if (weapon == null || dinfo.Weapon != weapon.def)
            return;

        CompEnchantable comp = weapon.GetComp<CompEnchantable>();
        EnchantmentDef enchantment = comp?.enchantment;
        if (enchantment == null || comp.charges <= 0 || !Rand.Chance(enchantment.chance))
            return;

        comp.charges--;

        applying = true;
        try
        {
            enchantment.Worker.Apply(attacker, pawn, dinfo.Amount);
        }
        finally
        {
            applying = false;
        }
    }
}
