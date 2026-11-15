namespace ESCP.Core.Enchantments;

public class EnchantmentDef : Def
{
    public Type workerClass = typeof(EnchantmentWorker);
    public string adjective;
    public float magnitude = 1f;
    public float chance = 1f;
    public HediffDef hediff;
    public DamageDef extraDamage;

    private EnchantmentWorker worker;

    public EnchantmentWorker Worker
    {
        get
        {
            if (worker == null)
            {
                worker = (EnchantmentWorker)Activator.CreateInstance(workerClass);
                worker.def = this;
            }
            return worker;
        }
    }
}
