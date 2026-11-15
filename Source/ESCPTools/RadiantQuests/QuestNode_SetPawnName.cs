using RimWorld.QuestGen;

namespace ESCP.Core.RadiantQuests;

public class QuestNode_SetPawnName : QuestNode
{
    public SlateRef<Pawn> pawn;
    [NoTranslate]
    public SlateRef<string> name;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        Pawn target = pawn.GetValue(slate);
        string pawnName = name.GetValue(slate);
        if (target == null || pawnName.NullOrEmpty())
        {
            return;
        }

        int space = pawnName.IndexOf(' ');
        string first = space > 0 ? pawnName.Substring(0, space) : pawnName;
        string last = space > 0 ? pawnName.Substring(space + 1) : "";
        target.Name = new NameTriple(first, first, last);
    }
}
