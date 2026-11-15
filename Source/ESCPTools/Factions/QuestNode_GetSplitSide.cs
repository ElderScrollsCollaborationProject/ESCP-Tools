using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace ESCP.Factions
{
    public class QuestNode_GetSplitSide : QuestNode
    {
        public SlateRef<FactionSplitDef> split;
        public SlateRef<FactionDef> side;

        [NoTranslate]
        public SlateRef<string> storeAllyAs;

        [NoTranslate]
        public SlateRef<string> storeEnemyAs;

        protected override bool TestRunInt(Slate slate)
        {
            return SetVars(slate);
        }

        protected override void RunInt()
        {
            SetVars(QuestGen.slate);
        }

        private bool SetVars(Slate slate)
        {
            FactionSplitDef def = split.GetValue(slate);
            FactionDef joined = FactionSplitTracker.Instance.SideOf(def);
            FactionDef wanted = side.GetValue(slate);
            if (joined == null || wanted != null && wanted != joined)
                return false;

            Faction ally = Find.FactionManager.FirstFactionOfDef(joined);
            Faction enemy = Find.FactionManager.FirstFactionOfDef(joined == def.loyalFaction ? def.rebelFaction : def.loyalFaction);
            if (ally == null || enemy == null || ally.defeated || enemy.defeated || ally.HostileTo(Faction.OfPlayer) || !enemy.HostileTo(Faction.OfPlayer))
                return false;

            slate.Set(storeAllyAs.GetValue(slate), ally);
            slate.Set(storeEnemyAs.GetValue(slate), enemy);
            return true;
        }
    }
}
