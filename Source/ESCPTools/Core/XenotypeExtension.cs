using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ESCP.Core
{
    public class XenotypeExtension : DefModExtension
    {
        public List<XenotypeDef> whitelistedXenotypes;
        public List<XenotypeDef> blacklistedXenotypes;
        public List<XenotypeDef> craftableByXenotypes;
    }
}
