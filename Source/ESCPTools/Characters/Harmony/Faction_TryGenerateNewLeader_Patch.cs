using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP.Core
{
    [HarmonyPatch(typeof(Faction), "TryGenerateNewLeader")]
    public static class Faction_TryGenerateNewLeader_Patch
    {
        public static bool Prefix(Faction __instance, ref bool __result)
        {
            UniqueCharactersTracker tracker = UniqueCharactersTracker.Instance;
            if (tracker == null)
                return true;

            IEnumerable<CharacterDefWithRole<CharacterRole_FactionLeader>> leaderDefs =
                CharacterRoleUtils.GetAllWithRole<CharacterRole_FactionLeader>()
                    .Where(c => c.characterDef.faction == __instance.def && c.characterDef.InSelectedEra)
                    .OrderByDescending(c => c.role.seniority);

            foreach (CharacterDefWithRole<CharacterRole_FactionLeader> charWithRole in leaderDefs)
            {
                PawnGenerationRequest request = new PawnGenerationRequest(charWithRole.characterDef.pawnKind, __instance);
                Pawn leader = tracker.GetOrGenPawn(charWithRole.characterDef, request);

                if (leader != null && !leader.Dead && leader.Faction != null && leader.Faction != __instance && !leader.Faction.IsPlayer)
                    leader.SetFaction(__instance);

                if (leader == null || leader.Faction != __instance)
                    continue;

                if (!charWithRole.role.PawnIsValid(leader))
                    continue;

                charWithRole.role.ApplyRole(leader);
                __result = true;
                return false;
            }

            if (__instance.def.hidden || !__instance.def.fixedLeaderKinds.NullOrEmpty() || __instance.def.pawnGroupMakers == null
                || __instance.def.pawnGroupMakers.Any(g => g.options.Any(o => o.kind.factionLeader)))
                return true;

            PawnKindDef kind = __instance.def.pawnGroupMakers
                .SelectMany(g => g.options)
                .Select(o => o.kind)
                .Where(k => k.RaceProps.Humanlike)
                .OrderByDescending(k => k.combatPower)
                .FirstOrDefault();
            if (kind == null)
                return true;

            Pawn generic = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, __instance, forceGenerateNewPawn: true));
            if (!Find.WorldPawns.Contains(generic))
                Find.WorldPawns.PassToWorld(generic, RimWorld.Planet.PawnDiscardDecideMode.KeepForever);
            __instance.leader = generic;
            __result = true;
            return false;
        }
    }
}
