using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ESCP.Core
{
    public class UniqueCharactersTracker : WorldComponent
    {
        public static UniqueCharactersTracker Instance { get; private set; }

        private List<UniqueCharacter> characters = new List<UniqueCharacter>();
        private HashSet<ThingDef> spawnedUniqueThings = new HashSet<ThingDef>();

        private readonly Dictionary<CharacterDef, UniqueCharacter> charactersByDef = new Dictionary<CharacterDef, UniqueCharacter>();
        private readonly Dictionary<Pawn, UniqueCharacter> charactersByPawn = new Dictionary<Pawn, UniqueCharacter>();

        public UniqueCharactersTracker(World world) : base(world)
        {
            Instance = this;
        }

        public override void FinalizeInit(bool fromLoad)
        {
            base.FinalizeInit(fromLoad);
            Instance = this;

            if (fromLoad)
                RebuildDictionaries();
        }

        private void RebuildDictionaries()
        {
            charactersByDef.Clear();
            charactersByPawn.Clear();

            foreach (UniqueCharacter character in characters)
            {
                if (character.def != null)
                    charactersByDef[character.def] = character;
                if (character.pawn != null)
                    charactersByPawn[character.pawn] = character;
            }
        }

        public bool CharacterPawnExists(CharacterDef charDef)
        {
            return charactersByDef.TryGetValue(charDef, out UniqueCharacter character) && character.PawnExists();
        }

        public bool CharacterPawnExistsAlive(CharacterDef charDef)
        {
            return charactersByDef.TryGetValue(charDef, out UniqueCharacter character) && character.PawnExists() && !character.pawn.Dead;
        }

        public bool TryGetPawnCharacter(Pawn pawn, out UniqueCharacter character)
        {
            return charactersByPawn.TryGetValue(pawn, out character);
        }

        public bool IsUniquePawn(Pawn pawn)
        {
            return TryGetPawnCharacter(pawn, out _);
        }

        public bool CharacterPawnSpawned(CharacterDef charDef)
        {
            return charactersByDef.TryGetValue(charDef, out UniqueCharacter character) && character.PawnExists() && character.pawn.Spawned;
        }

        public bool IsUniqueThingCreated(ThingDef def)
        {
            return spawnedUniqueThings.Contains(def);
        }

        public void Notify_UniqueThingSpawned(ThingDef def)
        {
            spawnedUniqueThings.Add(def);
        }

        public void Notify_UniqueThingDestroyed(ThingDef def)
        {
            spawnedUniqueThings.Remove(def);
        }

        public Pawn GetOrGenPawn(CharacterDef charDef, PawnGenerationRequest? requestParams = null, Faction forcedFaction = null)
        {
            if (!charactersByDef.TryGetValue(charDef, out UniqueCharacter character))
            {
                character = new UniqueCharacter { def = charDef };
                characters.Add(character);
                charactersByDef[charDef] = character;
            }
            else if (character.PawnExists())
            {
                if (charDef.xenotype != null && character.pawn.genes != null && character.pawn.genes.Xenotype != charDef.xenotype)
                {
                    character.pawn.genes.SetXenotype(charDef.xenotype);
                    SingleMelaninGene.Trim(character.pawn, true);
                }

                return character.pawn;
            }

            PawnGenerationRequest request = requestParams ?? new PawnGenerationRequest(charDef.pawnKind);
            if (request.KindDef == null) request.KindDef = charDef.pawnKind;
            if (request.Faction == null)
                request.Faction = forcedFaction ?? (charDef.faction != null ? Find.FactionManager?.FirstFactionOfDef(charDef.faction) : null);
            request.ForceGenerateNewPawn = true;

            CharacterDefinitionUtils.ApplyRequestDefinitions(ref request, charDef.definitions);
            character.pawn = PawnGenerator.GeneratePawn(request);

            if (charDef.xenotype != null && character.pawn.genes != null)
            {
                character.pawn.genes.SetXenotype(charDef.xenotype);
                SingleMelaninGene.Trim(character.pawn, false);
            }

            CharacterDefinitionUtils.ApplyPawnDefinitions(character.pawn, charDef.definitions);
            charactersByPawn[character.pawn] = character;

            Find.WorldPawns.PassToWorld(character.pawn, PawnDiscardDecideMode.KeepForever);

            return character.pawn;
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref characters, "characters", LookMode.Deep);
            Scribe_Collections.Look(ref spawnedUniqueThings, "spawnedUniqueThings", LookMode.Def);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (characters == null)
                    characters = new List<UniqueCharacter>();
                if (spawnedUniqueThings == null)
                    spawnedUniqueThings = new HashSet<ThingDef>();
                Instance = this;
            }
        }
    }
}
