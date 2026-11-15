using RimWorld.Planet;
using RimWorld.QuestGen;

namespace ESCP.Core.RadiantQuests;

public class QuestNode_ReturnKidnapped : QuestNode
{
    public SlateRef<string> inSignal;
    public SlateRef<Pawn> pawn;
    public SlateRef<Faction> faction;
    public SlateRef<MapParent> mapParent;
    public SlateRef<string> outSignalReturned;
    public SlateRef<string> outSignalLost;

    protected override bool TestRunInt(Slate slate) => pawn.GetValue(slate) != null && faction.GetValue(slate) != null;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestGen.quest.AddPart(new QuestPart_ReturnKidnapped
        {
            inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal"),
            pawn = pawn.GetValue(slate),
            faction = faction.GetValue(slate),
            mapParent = mapParent.GetValue(slate),
            outSignalReturned = QuestGenUtility.HardcodedSignalWithQuestID(outSignalReturned.GetValue(slate)),
            outSignalLost = QuestGenUtility.HardcodedSignalWithQuestID(outSignalLost.GetValue(slate))
        });
    }
}

public class QuestPart_ReturnKidnapped : QuestPart
{
    public string inSignal;
    public Pawn pawn;
    public Faction faction;
    public MapParent mapParent;
    public string outSignalReturned;
    public string outSignalLost;

    public override IEnumerable<GlobalTargetInfo> QuestLookTargets
    {
        get
        {
            if (mapParent != null)
                yield return mapParent;
        }
    }

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag != inSignal)
            return;

        if (pawn == null || pawn.Dead || faction == null || !faction.kidnapped.KidnappedPawnsListForReading.Contains(pawn) || mapParent == null || !mapParent.HasMap)
        {
            if (!outSignalLost.NullOrEmpty())
                Find.SignalManager.SendSignal(new Signal(outSignalLost));
            return;
        }

        Map map = mapParent.Map;
        faction.kidnapped.RemoveKidnappedPawn(pawn);
        Find.WorldPawns.RemovePawn(pawn);
        GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(map.Center, map, 8), map);
        if (!outSignalReturned.NullOrEmpty())
            Find.SignalManager.SendSignal(new Signal(outSignalReturned, pawn.Named("SUBJECT")));
    }

    public override void Notify_FactionRemoved(Faction f)
    {
        if (faction == f)
            faction = null;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_References.Look(ref pawn, "pawn");
        Scribe_References.Look(ref faction, "faction");
        Scribe_References.Look(ref mapParent, "mapParent");
        Scribe_Values.Look(ref outSignalReturned, "outSignalReturned");
        Scribe_Values.Look(ref outSignalLost, "outSignalLost");
    }
}

public class QuestNode_FactionRelationToPlayer : QuestNode
{
    public SlateRef<string> inSignal;
    public SlateRef<Faction> faction;
    public SlateRef<FactionRelationKind> relationKind;

    protected override bool TestRunInt(Slate slate) => true;

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        QuestGen.quest.FactionRelationToPlayerChange(faction.GetValue(slate), relationKind.GetValue(slate), true, QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)));
    }
}
