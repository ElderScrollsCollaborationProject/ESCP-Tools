using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace ESCP.Core.RadiantQuests;

public class QuestNode_GiveTrait : QuestNode
{
    public SlateRef<string> inSignal;
    public SlateRef<Pawn> pawn;
    public SlateRef<TraitDef> trait;

    protected override bool TestRunInt(Slate slate) => true;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestGen.quest.AddPart(new QuestPart_GiveTrait
        {
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal"),
            pawn = pawn.GetValue(slate),
            trait = trait.GetValue(slate)
        });
    }
}

public class QuestPart_GiveTrait : QuestPart
{
    public string inSignal;
    public Pawn pawn;
    public TraitDef trait;

    public override IEnumerable<GlobalTargetInfo> QuestLookTargets
    {
        get
        {
            if (pawn != null)
                yield return pawn;
        }
    }

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag != inSignal || pawn == null || pawn.Dead || pawn.story == null || pawn.story.traits.HasTrait(trait))
            return;
        pawn.story.traits.GainTrait(new Trait(trait));
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_References.Look(ref pawn, "pawn");
        Scribe_Defs.Look(ref trait, "trait");
    }
}
