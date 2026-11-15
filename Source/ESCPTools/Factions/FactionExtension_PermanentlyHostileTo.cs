using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ESCP.Factions
{
    public class FactionExtension_PermanentlyHostileTo : DefModExtension
    {
        public List<FactionDef> hostileFactionDefs;

        public bool FactionIsHostileTo(FactionDef other) => hostileFactionDefs.Contains(other);
    }
}
