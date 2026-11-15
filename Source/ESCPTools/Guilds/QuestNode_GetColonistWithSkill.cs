using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace ESCP.Guilds
{
    public class QuestNode_GetColonistWithSkill : QuestNode
    {
        public SkillDef         skill;
        public int              minLevel = 1;
        public SlateRef<string> storeAs  = "colonist";

        protected override bool TestRunInt(Slate slate)
        {
            var pawn = FindPawn();
            if (pawn == null)
                return false;

            slate.Set(storeAs.GetValue(slate), pawn);
            return true;
        }

        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            var pawn  = FindPawn();
            if (pawn != null)
                slate.Set(storeAs.GetValue(slate), pawn);
        }

        private Pawn FindPawn()
        {
            return PawnsFinder.AllMaps_FreeColonistsSpawned
                .Where(p => !p.Downed
                         && !p.IsPrisoner
                         && p.skills != null
                         && p.skills.GetSkill(skill).Level >= minLevel)
                .RandomElementWithFallback();
        }
    }
}
