using System.Collections.Generic;
using Verse;
using RimWorld;

namespace ESCP_RaceTools
{
    class RitualObligationTargetWorker_FilledDunmerGrave : RitualObligationTargetFilter
    {
        public RitualObligationTargetWorker_FilledDunmerGrave()
        {
        }

        public RitualObligationTargetWorker_FilledDunmerGrave(RitualObligationTargetFilterDef def) : base(def)
		{
        }

        public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
        {
            yield return "ESCP_DunmerRitualTargetSeanceInfo".Translate(parent.ideo.Named("IDEO"));
        }

        public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
        {
            foreach (Thing thing in map.listerThings.ThingsInGroup(ThingRequestGroup.Grave))
            {
                if (thing is Building_AshBurialPit pit && pit.HasCorpse)
                {
                    yield return thing;
                }
            }
        }

        protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
        {
            return target.Thing is Building_AshBurialPit pit && pit.HasCorpse;
        }
    }
}
