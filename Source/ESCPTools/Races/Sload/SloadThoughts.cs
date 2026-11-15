using RimWorld;
using Verse;

namespace ESCP.Sload
{
    public class Thought_SloadsInFaction : Thought_Situational
    {
        public override float MoodOffset() => BaseMoodOffset * SloadUtility.SloadsInFaction(pawn.Faction);
    }

    public class Thought_SloadsOnMap : Thought_Situational
    {
        public override float MoodOffset() => BaseMoodOffset * SloadUtility.SloadsOnMap(pawn.Map);
    }

    public class ThoughtWorker_Precept_SloadsInFaction : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p) => p.IsFreeNonSlaveColonist && SloadUtility.SloadsInFaction(p.Faction) > 0;
    }

    public class ThoughtWorker_Precept_NoSloadsInFaction : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p) => p.IsFreeNonSlaveColonist && SloadUtility.SloadsInFaction(p.Faction) == 0;
    }

    public class ThoughtWorker_Precept_SloadsOnMap : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p) => p.IsFreeNonSlaveColonist && SloadUtility.SloadsOnMap(p.Map) > 0;
    }

    public class ThoughtWorker_Precept_NoSloadsOnMap : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p) => p.IsFreeNonSlaveColonist && p.Spawned && SloadUtility.SloadsOnMap(p.Map) == 0;
    }

    public class ThoughtWorker_Precept_ThrassianPlague : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p) => p.IsFreeNonSlaveColonist && p.health.hediffSet.HasHediff(SloadDefOf.ESCP_Hediff_ThrassianPlague);
    }

    public class ThoughtWorker_Precept_SloadOpinion : ThoughtWorker_Precept_Social
    {
        protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn) => SloadUtility.IsSload(otherPawn);
    }

    public class ThoughtWorker_Precept_SloadLeather : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (p.apparel == null)
                return false;

            string label = null;
            int count = 0;
            List<Apparel> worn = p.apparel.WornApparel;
            for (int i = 0; i < worn.Count; i++)
            {
                if (worn[i].Stuff == SloadDefOf.ESCP_Leather_Sload)
                {
                    label ??= worn[i].def.label;
                    count++;
                }
            }
            if (count == 0)
                return false;
            return ThoughtState.ActiveAtStage(System.Math.Min(count, 4) - 1, label);
        }
    }

    public class GoodwillSituationWorker_Sload : GoodwillSituationWorker
    {
        public override string GetPostProcessedLabel(Faction other)
        {
            return Applies(Faction.OfPlayer, other) ? "MemeGoodwillImpact_Player".Translate(base.GetPostProcessedLabel(other)) : "MemeGoodwillImpact_Other".Translate(base.GetPostProcessedLabel(other));
        }

        public override int GetNaturalGoodwillOffset(Faction other)
        {
            return Applies(Faction.OfPlayer, other) || Applies(other, Faction.OfPlayer) ? def.naturalGoodwillOffset : 0;
        }

        private bool Applies(Faction faction, Faction sloads)
        {
            PreceptDef precept = ESCP_RaceTools.IdeoProperties.Get(def)?.requiredPreceptDef;
            return precept != null && SloadUtility.IsSloadFaction(sloads) && faction.ideos?.PrimaryIdeo?.HasPrecept(precept) == true;
        }
    }
}
