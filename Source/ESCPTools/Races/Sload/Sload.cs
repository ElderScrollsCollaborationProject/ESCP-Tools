using ESCP_NecromanticThralls;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ESCP.Sload
{
    [DefOf]
    public static class SloadDefOf
    {
        [MayRequire("Rick.ESCP.Sloads")]
        public static ThingDef ESCP_Race_Sload;
        [MayRequire("Rick.ESCP.Sloads")]
        public static ThingDef ESCP_Leather_Sload;
        [MayRequire("Rick.ESCP.Sloads")]
        public static ThingDef ESCP_Thing_ThrassianMiasma;
        [MayRequire("Rick.ESCP.Sloads")]
        public static HediffDef ESCP_Hediff_ThrassianPlague;
        [MayRequireIdeology]
        [MayRequire("Rick.ESCP.Sloads")]
        public static PreceptDef ESCP_SloadThrassianImmunity_Immune;

        static SloadDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SloadDefOf));
        }
    }

    public static class SloadUtility
    {
        public static bool IsSload(Pawn pawn) => pawn != null && pawn.def == SloadDefOf.ESCP_Race_Sload;

        public static bool IsSloadFaction(Faction faction) => faction?.def.basicMemberKind?.race == SloadDefOf.ESCP_Race_Sload;

        public static int SloadsInFaction(Faction faction)
        {
            if (faction == null)
                return 0;
            return Count(PawnsFinder.AllMaps_SpawnedPawnsInFaction(faction));
        }

        public static int SloadsOnMap(Map map)
        {
            if (map == null)
                return 0;
            return Count(map.mapPawns.AllPawnsSpawned);
        }

        private static int Count(IReadOnlyList<Pawn> pawns)
        {
            int count = 0;
            for (int i = 0; i < pawns.Count; i++)
            {
                if (pawns[i].def == SloadDefOf.ESCP_Race_Sload)
                    count++;
            }
            return count;
        }

        public static bool CanCatchPlague(Pawn pawn)
        {
            if (!pawn.RaceProps.IsFlesh || ThrallUtility.PawnIsThrall(pawn))
                return false;
            if (ESCP_RaceTools.RaceProperties.Get(pawn.def)?.thrassianPlagueImmune == true)
                return false;
            return pawn.Ideo == null || !pawn.Ideo.HasPrecept(SloadDefOf.ESCP_SloadThrassianImmunity_Immune);
        }

        public static void Infect(Pawn pawn, float severity)
        {
            if (CanCatchPlague(pawn))
                HealthUtility.AdjustSeverity(pawn, SloadDefOf.ESCP_Hediff_ThrassianPlague, severity);
        }
    }

    public class ThrassianMiasma : Thing
    {
        private int ticksLeft = -1;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (ticksLeft < 0)
                ticksLeft = Rand.Range(960, 1920);
        }

        public override void TickRare()
        {
            ticksLeft -= GenTicks.TickRareInterval;
            if (ticksLeft <= 0)
            {
                Destroy();
                return;
            }

            List<Thing> things = Position.GetThingList(Map);
            for (int i = things.Count - 1; i >= 0; i--)
            {
                if (things[i] is Pawn pawn)
                    SloadUtility.Infect(pawn, 0.05f);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksLeft, "ticksLeft", -1);
        }
    }

    public class MapComponent_ThrassianFog : MapComponent
    {
        private int endTick;

        public MapComponent_ThrassianFog(Map map) : base(map)
        {
        }

        public bool Active => Find.TickManager.TicksGame < endTick;

        public int DaysLeft => Mathf.CeilToInt((endTick - Find.TickManager.TicksGame) / (float)GenDate.TicksPerDay);

        public void Extend(int days)
        {
            endTick = Mathf.Max(endTick, Find.TickManager.TicksGame) + days * GenDate.TicksPerDay;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref endTick, "endTick");
        }
    }

    public class CompProperties_AbilityThrassianFog : CompProperties_AbilityEffect
    {
        public int days = 5;

        public CompProperties_AbilityThrassianFog()
        {
            compClass = typeof(CompAbilityEffect_ThrassianFog);
        }
    }

    public class CompAbilityEffect_ThrassianFog : CompAbilityEffect
    {
        public new CompProperties_AbilityThrassianFog Props => (CompProperties_AbilityThrassianFog)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            parent.pawn.Map.GetComponent<MapComponent_ThrassianFog>().Extend(Props.days);
            Messages.Message("ESCP_Sload_ThrassianFogReleased".Translate(Props.days), parent.pawn, MessageTypeDefOf.PositiveEvent);
        }
    }

    public class Alert_ThrassianFog : Alert
    {
        public Alert_ThrassianFog()
        {
            defaultLabel = "ESCP_Sload_ThrassianFogAlert".Translate();
            defaultPriority = AlertPriority.Medium;
        }

        private MapComponent_ThrassianFog Fog => Find.CurrentMap?.GetComponent<MapComponent_ThrassianFog>();

        public override AlertReport GetReport() => Fog?.Active == true;

        public override TaggedString GetExplanation() => "ESCP_Sload_ThrassianFogAlertDesc".Translate(Fog?.DaysLeft ?? 0);
    }

    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "GenerateRaidLoot")]
    public static class IncidentWorker_RaidEnemy_GenerateRaidLoot_ThrassianFog_Patch
    {
        public static void Postfix(IncidentParms parms, List<Pawn> pawns)
        {
            if (parms.target is not Map map || map.GetComponent<MapComponent_ThrassianFog>()?.Active != true)
                return;

            for (int i = 0; i < pawns.Count; i++)
                SloadUtility.Infect(pawns[i], Rand.Range(0.05f, 0.25f));
        }
    }

    public class IncidentWorker_ThrassianPlague : IncidentWorker_DiseaseHuman
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
                return false;
            Ideo ideo = Faction.OfPlayer.ideos?.PrimaryIdeo;
            if (ideo != null && ideo.HasPrecept(SloadDefOf.ESCP_SloadThrassianImmunity_Immune))
                return false;
            return Find.FactionManager.AllFactionsListForReading.Any(f => SloadUtility.IsSloadFaction(f) && f.HostileTo(Faction.OfPlayer));
        }
    }

    public class CompProperties_UseEffectThrassianElixir : CompProperties_UseEffect
    {
        public HediffDef hediff;
        public AbilityDef refreshAbility;

        public CompProperties_UseEffectThrassianElixir()
        {
            compClass = typeof(CompUseEffect_ThrassianElixir);
        }
    }

    public class CompUseEffect_ThrassianElixir : CompUseEffect
    {
        public CompProperties_UseEffectThrassianElixir Props => (CompProperties_UseEffectThrassianElixir)props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (!SloadUtility.IsSload(usedBy))
            {
                Map map = usedBy.MapHeld;
                IntVec3 cell = usedBy.PositionHeld;
                usedBy.Kill(null);
                if (map != null)
                    GenExplosion.DoExplosion(cell, map, 3f, DamageDefOf.Extinguish, null, postExplosionSpawnThingDef: SloadDefOf.ESCP_Thing_ThrassianMiasma, postExplosionSpawnChance: 1f);
                return;
            }
            if (Props.hediff != null)
                usedBy.health.AddHediff(Props.hediff);
            if (Props.refreshAbility != null)
                usedBy.abilities.GetAbility(Props.refreshAbility)?.ResetCooldown();
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (!SloadUtility.IsSload(p))
                return true;
            if (Props.hediff != null && p.health.hediffSet.HasHediff(Props.hediff))
                return "ESCP_Sload_ElixirAlreadyUsed".Translate(p.Named("PAWN"));
            if (Props.refreshAbility != null && (p.abilities.GetAbility(Props.refreshAbility) is not { } ability || !ability.OnCooldown))
                return "ESCP_Sload_ElixirNotNeeded".Translate(p.Named("PAWN"));
            return true;
        }
    }

    public class CompProperties_AbilityImproveThrall : CompProperties_AbilityEffect
    {
        public SkillDef skill;
        public float chancePerLevel = 0.05f;

        public CompProperties_AbilityImproveThrall()
        {
            compClass = typeof(CompAbilityEffect_ImproveThrall);
        }
    }

    public class CompAbilityEffect_ImproveThrall : CompAbilityEffect
    {
        public new CompProperties_AbilityImproveThrall Props => (CompProperties_AbilityImproveThrall)props;

        private float Chance => Props.chancePerLevel * parent.pawn.skills.GetSkill(Props.skill ?? SkillDefOf.Intellectual).Level;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.Pawn?.skills == null)
                return;

            float chance = Chance;
            foreach (SkillRecord skill in target.Pawn.skills.skills)
            {
                if (skill.TotallyDisabled || skill.Level >= SkillRecord.MaxLevel)
                    continue;
                if (skill.passion == Passion.Major || Rand.Chance(skill.passion == Passion.Minor ? chance * 2f : chance))
                    skill.Level++;
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            HediffComp_Enthralled thrall = target.Pawn?.health.hediffSet.GetFirstHediffOfDef(ESCP_NecromanticThralls.HediffDefOf.ESCP_NecromanticThralls_Enthralled)?.TryGetComp<HediffComp_Enthralled>();
            return thrall?.Master == parent.pawn && base.Valid(target, throwMessages);
        }

        public override string ExtraTooltipPart()
        {
            float chance = Chance;
            return "ESCP_Sload_ImproveThrallChance".Translate(Mathf.Clamp01(chance * 2f).ToStringPercent(), Mathf.Clamp01(chance).ToStringPercent());
        }
    }
}
