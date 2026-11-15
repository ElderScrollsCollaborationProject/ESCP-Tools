using Verse;
using System.Collections.Generic;

namespace ESCP_RaceTools
{
    public class BeastMaster : DefModExtension
    {

        public int maxNumberOfPawns = 5;
        public float maxTotalCombatPower = 120f;

        public List<string> pawnKinds;

        public HediffDef hediffToAdd;
        public float hediffToAddSeverity = 1f;

        public bool bond = true;

        public static BeastMaster Get(Def def)
        {
            return def.GetModExtension<BeastMaster>();
        }
    }
}
