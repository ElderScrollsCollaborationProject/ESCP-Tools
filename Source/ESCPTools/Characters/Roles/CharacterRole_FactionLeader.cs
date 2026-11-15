using RimWorld;
using Verse;

namespace ESCP.Core
{
    public class CharacterRole_FactionLeader : CharacterRole
    {
        public int seniority;

        public override bool PawnIsValid(Pawn pawn)
        {
            if (pawn.Dead || pawn.IsPrisonerOfColony)
                return false;
            return base.PawnIsValid(pawn);
        }

        public override void ApplyRole(Pawn pawn)
        {
            base.ApplyRole(pawn);

            Faction faction = pawn.Faction;
            if (faction == null)
            {
                Log.Warning($"[ESCP] CharacterRole_FactionLeader: pawn {pawn.LabelShort} has no faction, cannot assign as leader.");
                return;
            }

            if (pawn.RaceProps.IsFlesh)
                pawn.relations.everSeenByPlayer = true;

            Ideo factionIdeo = faction.ideos?.PrimaryIdeo;
            if (factionIdeo != null && pawn.Ideo != factionIdeo)
                pawn.ideo.SetIdeo(factionIdeo);

            faction.leader = pawn;
        }
    }
}
