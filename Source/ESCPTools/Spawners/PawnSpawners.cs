using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace ESCP.Spawners
{
    public class PawnSpawnExtension : DefModExtension
    {
        public PawnKindDef kindToSpawn;
        public bool spawnAsPlayerFaction = true;
        public FactionDef faction;
        public MentalStateDef mentalStateDef;
        public bool attackTarget = true;
        public int numberToSpawn = 1;
        public float chance = 1f;
        public SoundDef soundDef;
        public RecordDef recordDef;
        [MustTranslate]
        public string message;
        public MessageTypeDef messageTypeDef;

        public bool TrySpawn(Thing spawner, Map map, Thing target = null, bool placeExactly = false, Pawn summoner = null)
        {
            if (kindToSpawn == null || map == null || !Rand.Chance(chance))
                return false;

            Faction spawnFaction = summoner?.Faction ?? (spawnAsPlayerFaction ? Faction.OfPlayer : faction != null ? Find.FactionManager.FirstFactionOfDef(faction) : null);
            Lord lord = summoner?.GetLord();
            for (int i = 0; i < numberToSpawn; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(kindToSpawn, spawnFaction);
                if (placeExactly)
                    GenSpawn.Spawn(pawn, spawner.Position, map);
                else
                    GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(spawner.Position, map, 1), map);
                lord?.AddPawn(pawn);

                if (attackTarget && target != null)
                    pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.AttackMelee, target), JobCondition.InterruptForced);
                if (mentalStateDef != null)
                    pawn.mindState.mentalStateHandler.TryStartMentalState(mentalStateDef);
                if (i == 0 && !message.NullOrEmpty())
                    Messages.Message(message, pawn, messageTypeDef ?? MessageTypeDefOf.NegativeEvent);
            }
            soundDef?.PlayOneShot(new TargetInfo(spawner.Position, map));
            return true;
        }
    }

    public class DeathActionWorker_SpawnPawns : DeathActionWorker
    {
        public override void PawnDied(Corpse corpse, Lord prevLord)
        {
            corpse.InnerPawn.def.GetModExtension<PawnSpawnExtension>()?.TrySpawn(corpse, corpse.MapHeld);
        }
    }

    public class HediffCompProperties_SpawnPawnsOnDeath : HediffCompProperties
    {
        public HediffCompProperties_SpawnPawnsOnDeath()
        {
            compClass = typeof(HediffComp_SpawnPawnsOnDeath);
        }
    }

    public class HediffComp_SpawnPawnsOnDeath : HediffComp
    {
        public override void Notify_PawnKilled()
        {
            Def.GetModExtension<PawnSpawnExtension>()?.TrySpawn(Pawn, Pawn.MapHeld);
        }
    }

    public class Building_TrapSpawnPawns : Building_Trap
    {
        protected override void SpringSub(Pawn p)
        {
            Map map = Map;
            def.GetModExtension<PawnSpawnExtension>()?.TrySpawn(this, map, p);
            if (!Destroyed)
                Destroy();
        }
    }

    public class Projectile_SpawnPawns : Projectile_Explosive
    {
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            PawnSpawnExtension ext = def.GetModExtension<PawnSpawnExtension>();
            if (ext != null && ext.TrySpawn(this, Map, intendedTarget.Thing, summoner: launcher as Pawn) && ext.recordDef != null && launcher is Pawn p)
                p.records.Increment(ext.recordDef);
            landed = true;
            Destroy();
        }
    }

    public class CompProperties_SpawnPawnsOnDestroy : CompProperties
    {
        public bool notRot = true;
        public bool notFlame = true;
        public bool notDeterioration = true;
        public bool notToxicFallout = true;

        public CompProperties_SpawnPawnsOnDestroy()
        {
            compClass = typeof(CompSpawnPawnsOnDestroy);
        }
    }

    public class CompSpawnPawnsOnDestroy : ThingComp
    {
        private bool ignoredDamage;

        public CompProperties_SpawnPawnsOnDestroy Props => (CompProperties_SpawnPawnsOnDestroy)props;

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            ignoredDamage = (dinfo.Def == DamageDefOf.Rotting && Props.notRot) || (dinfo.Def == DamageDefOf.Deterioration && Props.notDeterioration) || (dinfo.Def == DamageDefOf.Flame && Props.notFlame);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (previousMap == null || ignoredDamage || mode == DestroyMode.Vanish || mode == DestroyMode.Deconstruct)
                return;
            if (Props.notToxicFallout && previousMap.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout))
                return;

            parent.def.GetModExtension<PawnSpawnExtension>()?.TrySpawn(parent, previousMap, placeExactly: true);
        }
    }
}
