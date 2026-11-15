namespace ESCP.Core;

public class HediffCompProperties_DeathNotice : HediffCompProperties
{
    [MustTranslate]
    public string letterLabel;
    [MustTranslate]
    public string letterText;
    public LetterDef letterDef;
    public ThoughtDef colonyThought;

    public HediffCompProperties_DeathNotice()
    {
        compClass = typeof(HediffComp_DeathNotice);
    }
}

public class HediffComp_DeathNotice : HediffComp
{
    public HediffCompProperties_DeathNotice Props => (HediffCompProperties_DeathNotice)props;

    public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
    {
        Pawn pawn = Pawn;
        if (pawn.Faction == null || !pawn.Faction.IsPlayer)
            return;

        if (Props.colonyThought != null)
        {
            foreach (Pawn colonist in PawnsFinder.AllMaps_FreeColonistsSpawned)
            {
                if (colonist != pawn)
                    colonist.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.colonyThought, pawn);
            }
        }

        if (!Props.letterLabel.NullOrEmpty())
        {
            Find.LetterStack.ReceiveLetter(Props.letterLabel.Formatted(pawn.Named("PAWN")), Props.letterText.Formatted(pawn.Named("PAWN")),
                Props.letterDef ?? LetterDefOf.NeutralEvent, new LookTargets(pawn.Corpse));
        }
    }
}
