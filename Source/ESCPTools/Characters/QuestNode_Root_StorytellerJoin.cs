using RimWorld.QuestGen;

namespace ESCP.Core;

public class StorytellerExtension_Joiner : DefModExtension
{
    public CharacterDef characterDef;
}

public class QuestNode_Root_StorytellerJoin : QuestNode_Root_WandererJoin
{
    private const int TimeoutTicks = 30000;

    private string signalAccept;
    private string signalReject;

    protected override bool TestRunInt(Slate slate)
    {
        CharacterDef character = Find.Storyteller.def.GetModExtension<StorytellerExtension_Joiner>()?.characterDef;
        return character != null && !UniqueCharactersTracker.Instance.CharacterPawnExists(character) && base.TestRunInt(slate);
    }

    protected override void RunInt()
    {
        base.RunInt();
        Quest quest = QuestGen.quest;
        quest.Delay(TimeoutTicks, delegate
        {
            QuestGen_End.End(quest, QuestEndOutcome.Fail);
        });
    }

    public override Pawn GeneratePawn()
    {
        Pawn pawn = UniqueCharactersTracker.Instance.GetOrGenPawn(Find.Storyteller.def.GetModExtension<StorytellerExtension_Joiner>().characterDef);
        if (pawn.Faction != null)
            pawn.SetFaction(null);
        return pawn;
    }

    public override Pawn GeneratePawn_NewTemp(Map map)
    {
        return GeneratePawn();
    }

    protected override void AddSpawnPawnQuestParts(Quest quest, Map map, Pawn pawn)
    {
        signalAccept = QuestGenUtility.HardcodedSignalWithQuestID("Accept");
        signalReject = QuestGenUtility.HardcodedSignalWithQuestID("Reject");

        quest.Signal(signalAccept, delegate
        {
            quest.SetFaction(Gen.YieldSingle(pawn), Faction.OfPlayer);
            quest.PawnsArrive(Gen.YieldSingle(pawn), null, map.Parent);
            QuestGen_End.End(quest, QuestEndOutcome.Success);
        });

        quest.Signal(signalReject, delegate
        {
            quest.GiveDiedOrDownedThoughts(pawn, PawnDiedOrDownedThoughtsKind.DeniedJoining);
            QuestGen_End.End(quest, QuestEndOutcome.Fail);
        });
    }

    [Obsolete]
    public override void SendLetter(Quest quest, Pawn pawn)
    {
        var letter = (ChoiceLetter_AcceptJoiner)LetterMaker.MakeLetter("ESCP_StorytellerJoin_Label".Translate(pawn.Named("PAWN")), "ESCP_StorytellerJoin_Text".Translate(pawn.Named("PAWN")).AdjustedFor(pawn), LetterDefOf.AcceptJoiner);
        letter.signalAccept = signalAccept;
        letter.signalReject = signalReject;
        letter.quest = quest;
        letter.overrideMap = Find.AnyPlayerHomeMap;
        letter.StartTimeout(TimeoutTicks);
        Find.LetterStack.ReceiveLetter(letter);
    }
}
