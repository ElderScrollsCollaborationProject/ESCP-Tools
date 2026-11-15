using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ESCP.Core
{
    public static class DebugActionsUniqueCharactersSpawning
    {
        [DebugAction("ESCP", "Spawn All Characters Near Colony", allowedGameStates = AllowedGameStates.Playing)]
        private static void SpawnAllCharactersNearColony()
        {
            if (UniqueCharactersTracker.Instance == null)
            {
                Log.Message("[ESCP] UniqueCharactersTracker.Instance is null.");
                return;
            }

            Map map = Find.CurrentMap;
            if (map == null)
            {
                Log.Message("[ESCP] No current map.");
                return;
            }

            IntVec3 spawnCell = map.Center;

            foreach (CharacterDef def in DefDatabase<CharacterDef>.AllDefsListForReading)
            {
                Pawn pawn = UniqueCharactersTracker.Instance.GetOrGenPawn(def);
                if (pawn == null) continue;

                if (!pawn.Spawned)
                {
                    IntVec3 cell = CellFinder.RandomClosewalkCellNear(spawnCell, map, 10);
                    GenSpawn.Spawn(pawn, cell, map);
                    Log.Message($"[ESCP] Spawned {pawn.LabelShort} at {cell}.");
                }
                else
                {
                    Log.Message($"[ESCP] {pawn.LabelShort} is already spawned.");
                }
            }
        }

        [DebugAction("ESCP", "Spawn Character", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SpawnCharacter()
        {
            var options = new List<DebugMenuOption>();
            foreach (var group in DefDatabase<CharacterDef>.AllDefsListForReading.GroupBy(def => def.faction).OrderBy(g => g.Key?.label ?? ""))
            {
                List<CharacterDef> characters = group.ToList();
                string label = (group.Key?.LabelCap.ToString() ?? "No faction") + " (" + characters.Count + ")";
                options.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, () => ShowCharacters(characters, label)));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, "Factions"));
        }

        private static void ShowCharacters(List<CharacterDef> characters, string title)
        {
            var options = new List<DebugMenuOption> { new DebugMenuOption("Back", DebugMenuOptionMode.Action, SpawnCharacter) };
            foreach (CharacterDef def in characters.OrderBy(NameOf))
            {
                options.Add(new DebugMenuOption(NameOf(def), DebugMenuOptionMode.Tool, () =>
                {
                    Pawn pawn = UniqueCharactersTracker.Instance?.GetOrGenPawn(def);
                    if (pawn == null || pawn.Dead || pawn.Spawned)
                    {
                        Messages.Message(pawn == null ? "Could not generate " + NameOf(def) + "." : NameOf(def) + " is " + (pawn.Dead ? "dead." : "already on a map."), MessageTypeDefOf.RejectInput, false);
                        return;
                    }
                    if (pawn.IsWorldPawn())
                        Find.WorldPawns.RemovePawn(pawn);
                    GenSpawn.Spawn(pawn, UI.MouseCell(), Find.CurrentMap);
                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options, title));
        }

        private static string NameOf(CharacterDef def)
        {
            CharacterStoryDefinition story = def.definitions.OfType<CharacterStoryDefinition>().FirstOrDefault();
            return story == null ? def.defName : (story.firstName + " " + story.lastName).Trim();
        }

        [DebugAction("ESCP", "Regenerate All Characters", allowedGameStates = AllowedGameStates.PlayingOnWorld | AllowedGameStates.Playing)]
        private static void RegenerateAllCharacters()
        {
            if (UniqueCharactersTracker.Instance == null)
            {
                Log.Message("[ESCP] UniqueCharactersTracker.Instance is null.");
                return;
            }

            foreach (CharacterDef def in DefDatabase<CharacterDef>.AllDefsListForReading)
            {
                Pawn pawn = UniqueCharactersTracker.Instance.GetOrGenPawn(def);
                Log.Message($"[ESCP] Character '{def.defName}': {(pawn != null ? pawn.LabelShort : "FAILED")}");
            }
        }
    }
}
