using RimWorld.QuestGen;
using Verse;

namespace ESCP.Core.RadiantQuests;

public class QuestNode_CharacterHasSpawned : QuestNode
{
    public SlateRef<CharacterDef> characterDef;
    public QuestNode node;
    public QuestNode elseNode;

    protected override bool TestRunInt(Slate slate) => true;

    protected override void RunInt()
    {
        CharacterDef def = characterDef.GetValue(QuestGen.slate);
        bool hasSpawned = def != null && (!def.InSelectedEra || UniqueCharactersTracker.Instance.CharacterPawnExists(def));
        if (hasSpawned)
        {
            node?.Run();
        }
        else
        {
            elseNode?.Run();
        }
    }
}
