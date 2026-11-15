using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ESCP.Core
{
    public class CharacterDef : Def
    {
        public PawnKindDef pawnKind;
        public XenotypeDef xenotype;
        public FactionDef faction;
        public List<CharacterBaseDefinition> definitions = new List<CharacterBaseDefinition>();
        public List<CharacterRole> roles = new List<CharacterRole>();
        public List<int> eras = new List<int>();
        public bool exactEra;

        public bool InSelectedEra => exactEra ? eras.Contains(EraTracker.Era) : EraTracker.Allows(eras);
    }
}
