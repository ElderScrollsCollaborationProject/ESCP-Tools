using System.Text;
using LudeonTK;
using RimWorld;
using Verse;

namespace ESCP.Core
{
    public static class DebugActionsUniqueCharacters
    {
        [DebugAction("ESCP", "Log All Characters", allowedGameStates = AllowedGameStates.PlayingOnWorld | AllowedGameStates.Playing)]
        private static void LogAllCharacters()
        {
            if (UniqueCharactersTracker.Instance == null)
            {
                Log.Message("[ESCP] UniqueCharactersTracker.Instance is null.");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[ESCP] Unique Characters:");

            foreach (CharacterDef def in DefDatabase<CharacterDef>.AllDefsListForReading)
            {
                Pawn pawn = UniqueCharactersTracker.Instance.GetOrGenPawn(def);
                string pawnLabel = pawn != null ? pawn.LabelShort : "NULL";
                sb.AppendLine($"  {def.defName}: {pawnLabel}");

                foreach (CharacterRole role in def.roles)
                    sb.AppendLine($"    Role: {role.GetType().Name}");
            }

            Log.Message(sb.ToString());
        }

        [DebugAction("ESCP", "Log Character Factions", allowedGameStates = AllowedGameStates.PlayingOnWorld | AllowedGameStates.Playing)]
        private static void LogCharacterFactions()
        {
            if (UniqueCharactersTracker.Instance == null)
            {
                Log.Message("[ESCP] UniqueCharactersTracker.Instance is null.");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[ESCP] Character Factions:");

            foreach (CharacterDef def in DefDatabase<CharacterDef>.AllDefsListForReading)
            {
                Pawn pawn = UniqueCharactersTracker.Instance.GetOrGenPawn(def);
                if (pawn == null) continue;
                string faction = pawn.Faction?.Name ?? "no faction";
                bool isLeader = pawn.Faction?.leader == pawn;
                sb.AppendLine($"  {pawn.LabelShort} [{def.defName}] - {faction}{(isLeader ? " (LEADER)" : "")}");
            }

            Log.Message(sb.ToString());
        }
    }
}
