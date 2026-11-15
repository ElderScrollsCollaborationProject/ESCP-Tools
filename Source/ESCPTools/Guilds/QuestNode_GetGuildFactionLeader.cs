using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace ESCP.Guilds
{
    public class QuestNode_GetGuildFactionLeader : QuestNode
    {
        public string factionDefName;
        public SlateRef<string> storeLeaderAs  = "asker";
        public SlateRef<string> storeFactionAs = "faction";

        protected override bool TestRunInt(Slate slate)
        {
            var faction = Find.FactionManager.AllFactions
                .FirstOrDefault(f => f.def.defName == factionDefName);

            if (faction == null || !GuildUtility.EnsureLeader(faction))
                return false;

            slate.Set(storeLeaderAs.GetValue(slate),  faction.leader);
            slate.Set(storeFactionAs.GetValue(slate), faction);
            return true;
        }

        protected override void RunInt()
        {
            var slate   = QuestGen.slate;
            var faction = Find.FactionManager.AllFactions
                .First(f => f.def.defName == factionDefName);

            GuildUtility.EnsureLeader(faction);
            slate.Set(storeLeaderAs.GetValue(slate),  faction.leader);
            slate.Set(storeFactionAs.GetValue(slate), faction);
        }
    }
}
