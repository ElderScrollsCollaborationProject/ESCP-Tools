using RimWorld.QuestGen;
using Verse;

namespace ESCP.Core.RadiantQuests;

public class QuestNode_GenerateUniqueCharacterPawn : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;
    [NoTranslate]
    public SlateRef<string> addToList;
    public SlateRef<CharacterDef> characterDef;

    protected override bool TestRunInt(Slate slate)
    {
        RunInt();
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        CharacterDef def = characterDef.GetValue(slate);
        if (def == null || !def.InSelectedEra)
        {
            return;
        }

        Pawn pawn = UniqueCharactersTracker.Instance.GetOrGenPawn(def);

        string storeAsName = storeAs.GetValue(slate);
        if (!storeAsName.NullOrEmpty())
        {
            slate.Set(storeAsName, pawn);
        }

        string addToListName = addToList.GetValue(slate);
        if (!addToListName.NullOrEmpty())
        {
            List<Pawn> list = slate.Get<List<Pawn>>(addToListName) ?? new List<Pawn>();
            list.Add(pawn);
            slate.Set(addToListName, list);
        }
    }
}
