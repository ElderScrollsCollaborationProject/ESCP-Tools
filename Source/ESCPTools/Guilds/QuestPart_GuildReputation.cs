using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ESCP.Guilds
{
    public class QuestPart_GuildReputation : QuestPart
    {
        public string           inSignal;
        public GuildType        guild;
        public int              amount;
        public ThingSetMakerDef rewardPool;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            if (signal.tag != inSignal) return;
            GuildTracker.Current?.AddRep(guild, amount);
            ESCP.Core.Crime.CrimeUtility.Notify_ShadyContract(guild);

            Map map = Find.AnyPlayerHomeMap;
            if (rewardPool == null || map == null) return;

            List<Thing> things = rewardPool.root.Generate();
            if (things.NullOrEmpty()) return;

            DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(map), map, things, canRoofPunch: false);
            Messages.Message("ESCP_GuildRewardArrived".Translate(), new TargetInfo(things[0].Position, map), MessageTypeDefOf.PositiveEvent);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Values.Look(ref guild,    "guild");
            Scribe_Values.Look(ref amount,   "amount");
            Scribe_Defs.Look(ref rewardPool, "rewardPool");
        }
    }
}
