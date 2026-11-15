using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace ESCP.Core
{
    public class CharacterRole_Jarl : CharacterRole
    {
        public string settlement;
    }

    [HarmonyPatch(typeof(MapParent), nameof(MapParent.PostMapGenerate))]
    public static class MapParent_PostMapGenerate_Jarl_Patch
    {
        private const int SpawnRadius = 10;

        public static void Postfix(MapParent __instance)
        {
            if (!(__instance is Settlement settlement) || settlement.Faction == null || settlement.Faction.IsPlayer)
                return;

            foreach (CharacterDefWithRole<CharacterRole_Jarl> jarl in CharacterRoleUtils.GetAllWithRole<CharacterRole_Jarl>())
            {
                if (jarl.role.settlement != settlement.Name || !jarl.characterDef.InSelectedEra)
                    continue;

                PawnGenerationRequest request = new PawnGenerationRequest(jarl.characterDef.pawnKind, settlement.Faction);
                Pawn pawn = UniqueCharactersTracker.Instance.GetOrGenPawn(jarl.characterDef, request);
                if (pawn.Dead || pawn.Spawned || pawn.Faction != settlement.Faction)
                    continue;

                Map map = settlement.Map;
                GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(map.Center, map, SpawnRadius), map);
                map.lordManager.lords.Find(l => l.faction == settlement.Faction)?.AddPawn(pawn);
            }
        }
    }
}
