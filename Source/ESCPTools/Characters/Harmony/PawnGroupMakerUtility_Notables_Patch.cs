using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ESCP.Core
{
    [HarmonyPatch(typeof(PawnGroupMakerUtility), nameof(PawnGroupMakerUtility.GeneratePawns))]
    public static class PawnGroupMakerUtility_Notables_Patch
    {
        private const float Chance = 0.2f;

        public static IEnumerable<Pawn> Postfix(IEnumerable<Pawn> __result, PawnGroupMakerParms parms)
        {
            List<Pawn> pawns = __result.ToList();
            if (parms.faction != null && !parms.faction.IsPlayer && pawns.Any(p => p.RaceProps.Humanlike) && Rand.Chance(Chance))
            {
                Pawn notable = FindNotable(parms.faction);
                if (notable != null)
                    pawns.Add(notable);
            }
            return pawns;
        }

        private static Pawn FindNotable(Faction faction)
        {
            UniqueCharactersTracker tracker = UniqueCharactersTracker.Instance;
            if (tracker == null)
                return null;

            foreach (CharacterDef def in DefDatabase<CharacterDef>.AllDefsListForReading.Where(c => c.faction == faction.def && c.roles.NullOrEmpty() && c.InSelectedEra).InRandomOrder())
            {
                if (tracker.CharacterPawnExists(def))
                {
                    Pawn existing = tracker.GetOrGenPawn(def);
                    if (existing.Dead || existing.Spawned || existing.Faction != faction || existing.HostFaction != null || existing.GetCaravan() != null)
                        continue;
                    return existing;
                }
                return tracker.GetOrGenPawn(def, null, faction);
            }
            return null;
        }
    }
}
