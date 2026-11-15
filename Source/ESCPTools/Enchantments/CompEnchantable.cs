namespace ESCP.Core.Enchantments;

public class CompProperties_Enchantable : CompProperties
{
    public CompProperties_Enchantable()
    {
        compClass = typeof(CompEnchantable);
    }
}

public class CompEnchantable : ThingComp
{
    public const int MaxCharges = 200;
    public const int StartingCharges = 50;

    public EnchantmentDef enchantment;
    public int charges = StartingCharges;

    public void Recharge(int amount)
    {
        charges = Math.Min(charges + amount, MaxCharges);
    }

    public override string TransformLabel(string label)
    {
        return enchantment == null ? label : enchantment.adjective + " " + label;
    }

    public override string CompInspectStringExtra()
    {
        if (enchantment == null)
            return null;
        return "ESCP_Enchantment_Inspect".Translate(enchantment.label, enchantment.description) + "\n" + (charges > 0 ? "ESCP_Enchantment_Charges".Translate(charges, MaxCharges) : "ESCP_Enchantment_Drained".Translate());
    }

    public override float GetStatFactor(StatDef stat)
    {
        return enchantment != null && stat == StatDefOf.MarketValue ? 1.5f : 1f;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Defs.Look(ref enchantment, "enchantment");
        Scribe_Values.Look(ref charges, "charges", StartingCharges);
    }
}

[StaticConstructorOnStartup]
public static class EnchantableSetup
{
    static EnchantableSetup()
    {
        foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
        {
            if (def.IsWeapon && def.category == ThingCategory.Item && typeof(ThingWithComps).IsAssignableFrom(def.thingClass))
            {
                def.comps ??= new List<CompProperties>();
                def.comps.Add(new CompProperties_Enchantable());
            }
        }
    }
}
