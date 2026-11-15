using RimWorld.Planet;
using Verse;

namespace ESCP.Core
{
    public class UniqueCharacter : IExposable
    {
        public CharacterDef def;
        public Pawn pawn;

        public UniqueCharacter() { }

        public UniqueCharacter(CharacterDef def)
        {
            this.def = def;
        }

        public bool PawnExists()
        {
            if (pawn == null) return false;
            return !pawn.Discarded;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            if (Scribe.mode == LoadSaveMode.Saving && pawn != null && (pawn.Discarded || !pawn.Spawned && !pawn.IsWorldPawn() && pawn.ParentHolder == null))
                pawn = null;
            Scribe_References.Look(ref pawn, "pawn");
        }
    }
}
