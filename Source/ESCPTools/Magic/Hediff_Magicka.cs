using UnityEngine;

namespace ESCP.Core.Magic;

public class Hediff_Magicka : HediffWithComps
{
    private const int RegenInterval = 60;
    private const float MaxPerIntellectual = 5f;

    public float current = -1f;

    public float Max => pawn.GetStatValue(MagicDefOf.ESCP_MagickaMax) + (pawn.skills?.GetSkill(SkillDefOf.Intellectual).Level ?? 0) * MaxPerIntellectual;

    public override string LabelInBrackets => Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(Max);

    public override void PostAdd(DamageInfo? dinfo)
    {
        base.PostAdd(dinfo);
        if (current < 0f)
            current = Max;
    }

    public override void PostTickInterval(int delta)
    {
        base.PostTickInterval(delta);
        if (pawn.IsHashIntervalTick(RegenInterval, delta))
            current = Mathf.Min(current + pawn.GetStatValue(MagicDefOf.ESCP_MagickaRegen), Max);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        if (pawn.IsColonistPlayerControlled)
            yield return new Gizmo_Magicka(this);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref current, "current", -1f);
    }
}

[StaticConstructorOnStartup]
public class Gizmo_Magicka : Gizmo
{
    private static readonly Texture2D FillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.35f, 0.75f));
    private static readonly Texture2D EmptyTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.05f, 0.05f, 0.1f));

    private readonly Hediff_Magicka magicka;

    public Gizmo_Magicka(Hediff_Magicka magicka)
    {
        this.magicka = magicka;
        Order = -90f;
    }

    public override float GetWidth(float maxWidth) => 136f;

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
        Widgets.DrawWindowBackground(rect);
        Rect inner = rect.ContractedBy(6f);

        Text.Font = GameFont.Tiny;
        Widgets.Label(new Rect(inner.x, inner.y, inner.width, 20f), "ESCP_Magicka".Translate());

        Rect bar = new Rect(inner.x, inner.y + 24f, inner.width, inner.height - 24f);
        Widgets.FillableBar(bar, magicka.current / Mathf.Max(magicka.Max, 1f), FillTex, EmptyTex, false);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(bar, magicka.LabelInBrackets);
        Text.Anchor = TextAnchor.UpperLeft;
        Text.Font = GameFont.Small;

        TooltipHandler.TipRegion(rect, "ESCP_MagickaDesc".Translate());
        return new GizmoResult(GizmoState.Clear);
    }
}

public static class MagickaUtility
{
    public static Hediff_Magicka For(Pawn pawn)
    {
        if (pawn.health.hediffSet.GetFirstHediffOfDef(MagicDefOf.ESCP_Hediff_Magicka) is Hediff_Magicka magicka)
            return magicka;
        return (Hediff_Magicka)pawn.health.AddHediff(MagicDefOf.ESCP_Hediff_Magicka);
    }

    public static void HealInjuries(Pawn pawn, float amount)
    {
        List<Hediff_Injury> injuries = new List<Hediff_Injury>();
        pawn.health.hediffSet.GetHediffs(ref injuries, h => h.CanHealNaturally() || h.CanHealFromTending());
        for (int i = 0; i < injuries.Count && amount > 0f; i++)
        {
            float healed = Math.Min(amount, injuries[i].Severity);
            injuries[i].Heal(healed);
            amount -= healed;
        }
    }
}

[DefOf]
public static class MagicDefOf
{
    public static HediffDef ESCP_Hediff_Magicka;
    public static StatDef ESCP_MagickaMax;
    public static StatDef ESCP_MagickaRegen;
    public static JobDef ESCP_Job_StudyTome;

    static MagicDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(MagicDefOf));
    }
}
