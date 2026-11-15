using System.Collections.Generic;
using Verse;
using RimWorld;

namespace ESCP_RaceTools
{
    class RoleRequirement_RaceMulti : RoleRequirement
    {
        public override string GetLabel(Precept_Role role)
        {
            if (labelCached == null)
            {
                if (ESCP_RaceTools_ModSettings.IdeologyOverrideRace)
                {
                    labelCached = "ESCP_IdeoRequirementRace".Translate() + ": " + "ESCP_IdeoRequirementRaceAny".Translate();
                }
                else
                {
                    labelCached = "ESCP_IdeoRequirementRace".Translate() + ": " + xenotypes.Select(x => x.label).ToCommaList();
                }
            }
            return labelCached;
        }

        public override bool Met(Pawn p, Precept_Role role)
        {
            return ESCP_RaceTools_ModSettings.IdeologyOverrideRace || (p.genes != null && xenotypes.Contains(p.genes.Xenotype));
        }

        public List<XenotypeDef> xenotypes;

        [NoTranslate]
        private string labelCached;
    }
}
