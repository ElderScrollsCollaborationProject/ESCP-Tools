using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace ESCP.Guilds
{
    public class QuestNode_GuildReputation : QuestNode
    {
        public GuildType         guild;
        public int               amount;
        public ThingSetMakerDef  rewardPool;

        protected override bool TestRunInt(Slate slate) => true;

        protected override void RunInt()
        {
            QuestGen.quest.AddPart(new QuestPart_GuildReputation
            {
                inSignal   = QuestGen.slate.Get<string>("inSignal"),
                guild      = guild,
                amount     = amount,
                rewardPool = rewardPool
            });
        }
    }
}
