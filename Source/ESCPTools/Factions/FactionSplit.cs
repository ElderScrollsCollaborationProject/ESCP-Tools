using System.Collections.Generic;
using System.Linq;
using ESCP.Core;
using ESCP.Enlist;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace ESCP.Factions
{
    public class FactionSplitDef : Def
    {
        public FactionDef loyalFaction;
        public FactionDef rebelFaction;
        public int daysAfterStart = 30;
        public List<string> settlements = new List<string>();
        public CharacterDef slainCharacter;
        public string acceptLabel;
        public string refuseLabel;
        public string letterLabel;
        public string letterText;

        public ModOptionDef option;
        public List<int> eras = new List<int>();
        public List<FactionDef> loyalAllies = new List<FactionDef>();
        public int sideChoiceDelayDays = 3;
        public int joinGoodwill = 30;
        public float reinforcementChance = 0.5f;
        [MustTranslate] public string sideChoiceLabel;
        [MustTranslate] public string sideChoiceText;
        [MustTranslate] public string joinLoyalLabel;
        [MustTranslate] public string joinRebelLabel;
        [MustTranslate] public string stayOutLabel;

        public bool AllowsEra(int era) => era == 0 || eras.NullOrEmpty() || eras.Contains(era);
    }

    public class FactionSplitTracker : WorldComponent
    {
        private const int CheckInterval = 2500;
        private const int DefendBaseDelay = 25000;

        private List<FactionSplitDef> resolved = new List<FactionSplitDef>();
        private Dictionary<FactionSplitDef, int> sideChoiceTicks = new Dictionary<FactionSplitDef, int>();
        private Dictionary<FactionSplitDef, FactionDef> sides = new Dictionary<FactionSplitDef, FactionDef>();
        private List<FactionSplitDef> disabled = new List<FactionSplitDef>();
        private bool asking;

        public FactionSplitTracker(World world) : base(world)
        {
            foreach (FactionSplitDef def in DefDatabase<FactionSplitDef>.AllDefsListForReading)
            {
                if (def.option != null && !def.option.Enabled || !def.AllowsEra(ESCPToolsMod.Settings.characterEra))
                    disabled.Add(def);
            }
        }

        public static FactionSplitTracker Instance => Find.World.GetComponent<FactionSplitTracker>();

        public FactionDef SideOf(FactionSplitDef def) => sides.TryGetValue(def);

        public void Notify_RaidedBy(Faction raider, Map map, float points)
        {
            foreach (KeyValuePair<FactionSplitDef, FactionDef> side in sides)
            {
                FactionDef enemyDef = side.Value == side.Key.loyalFaction ? side.Key.rebelFaction : side.Key.loyalFaction;
                if (raider.def != enemyDef || !Rand.Chance(side.Key.reinforcementChance))
                    continue;
                Faction ally = Find.FactionManager.FirstFactionOfDef(side.Value);
                if (ally == null || ally.defeated || ally.PlayerRelationKind != FactionRelationKind.Ally)
                    return;
                IncidentParms parms = new IncidentParms
                {
                    target = map,
                    faction = ally,
                    points = points * 0.6f,
                    forced = true
                };
                Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidFriendly, Find.TickManager.TicksGame + Rand.Range(2500, 7500), parms);
                return;
            }
        }

        public override void WorldComponentTick()
        {
            if (asking || Find.TickManager.TicksGame % CheckInterval != 0)
                return;

            foreach (FactionSplitDef def in DefDatabase<FactionSplitDef>.AllDefsListForReading)
            {
                if (!resolved.Contains(def) && !disabled.Contains(def) && Find.TickManager.TicksGame >= def.daysAfterStart * GenDate.TicksPerDay && CanSplit(def))
                {
                    Ask(def);
                    return;
                }
                if (sideChoiceTicks.TryGetValue(def, out int tick) && Find.TickManager.TicksGame >= tick)
                {
                    sideChoiceTicks.Remove(def);
                    AskSide(def);
                    return;
                }
            }
        }

        private static bool CanSplit(FactionSplitDef def)
        {
            Faction loyal = Find.FactionManager.FirstFactionOfDef(def.loyalFaction);
            return loyal != null && !loyal.defeated
                && Find.FactionManager.FirstFactionOfDef(def.rebelFaction) == null
                && !DefDatabase<FactionSplitDef>.AllDefsListForReading.Any(other => other != def && other.loyalFaction == def.loyalFaction && Find.FactionManager.FirstFactionOfDef(other.rebelFaction) != null)
                && SecedingSettlements(def, loyal).Any();
        }

        private static IEnumerable<Settlement> SecedingSettlements(FactionSplitDef def, Faction loyal)
        {
            return Find.WorldObjects.Settlements.Where(s => s.Faction == loyal && def.settlements.Contains(s.Name));
        }

        private void Ask(FactionSplitDef def)
        {
            asking = true;
            DiaNode node = new DiaNode(def.description);
            node.options.Add(new DiaOption(def.acceptLabel)
            {
                action = () => Resolve(def, true),
                resolveTree = true
            });
            node.options.Add(new DiaOption(def.refuseLabel)
            {
                action = () => Resolve(def, false),
                resolveTree = true
            });
            Find.WindowStack.Add(new Dialog_NodeTree(node, true, false, def.LabelCap));
        }

        private void Resolve(FactionSplitDef def, bool split)
        {
            asking = false;
            resolved.Add(def);
            if (split)
            {
                Split(def);
                if (!def.sideChoiceText.NullOrEmpty())
                    sideChoiceTicks[def] = Find.TickManager.TicksGame + def.sideChoiceDelayDays * GenDate.TicksPerDay;
            }
        }

        private void AskSide(FactionSplitDef def)
        {
            Faction loyal = Find.FactionManager.FirstFactionOfDef(def.loyalFaction);
            Faction rebel = Find.FactionManager.FirstFactionOfDef(def.rebelFaction);
            if (loyal == null || rebel == null || loyal.defeated || rebel.defeated)
                return;

            asking = true;
            DiaNode node = new DiaNode(def.sideChoiceText);
            node.options.Add(new DiaOption(def.joinLoyalLabel)
            {
                action = () => ChooseSide(def, loyal, rebel),
                resolveTree = true
            });
            node.options.Add(new DiaOption(def.joinRebelLabel)
            {
                action = () => ChooseSide(def, rebel, loyal),
                resolveTree = true
            });
            node.options.Add(new DiaOption(def.stayOutLabel)
            {
                action = () => asking = false,
                resolveTree = true
            });
            Find.WindowStack.Add(new Dialog_NodeTree(node, true, false, def.sideChoiceLabel));
        }

        private void ChooseSide(FactionSplitDef def, Faction joined, Faction opposed)
        {
            asking = false;
            sides[def] = joined.def;
            bool loyalist = joined.def == def.loyalFaction;
            Join(joined, def.joinGoodwill);
            Oppose(opposed);
            foreach (FactionDef allyDef in def.loyalAllies)
            {
                Faction ally = Find.FactionManager.FirstFactionOfDef(allyDef);
                if (ally == null || ally.defeated)
                    continue;
                if (loyalist)
                    Join(ally, def.joinGoodwill);
                else
                    Oppose(ally);
            }

            WorldEnlistTracker enlist = WorldEnlistTracker.Instance;
            foreach (FactionEnlistOptionsDef options in opposed.GetEnlistOptions())
            {
                if (enlist.EnlistedTo(opposed, options))
                    enlist.Delist(opposed, options);
            }
            foreach (FactionEnlistOptionsDef options in joined.GetEnlistOptions())
            {
                if (!enlist.EnlistedTo(joined, options))
                    options.Worker.EnlistTo(joined);
            }
        }

        private static void Join(Faction faction, int goodwill)
        {
            int current = faction.GoodwillWith(Faction.OfPlayer);
            faction.TryAffectGoodwillWith(Faction.OfPlayer, current < 0 ? goodwill - current : goodwill, false, false);
        }

        private static void Oppose(Faction faction)
        {
            faction.TryAffectGoodwillWith(Faction.OfPlayer, -faction.GoodwillWith(Faction.OfPlayer) - 100, false, false);
        }

        public static void Split(FactionSplitDef def)
        {
            Faction loyal = Find.FactionManager.FirstFactionOfDef(def.loyalFaction);
            FactionGenerator.CreateFactionAndAddToManager(def.rebelFaction);
            Faction rebel = Find.FactionManager.FirstFactionOfDef(def.rebelFaction);

            foreach (Settlement stray in Find.WorldObjects.Settlements.Where(s => s.Faction == rebel).ToList())
                Find.WorldObjects.Remove(stray);

            foreach (Settlement settlement in SecedingSettlements(def, loyal).ToList())
            {
                settlement.SetFaction(rebel);
                if (settlement.HasMap)
                    TransferMap(settlement.Map, loyal, rebel);
            }

            UniqueCharactersTracker tracker = UniqueCharactersTracker.Instance;
            foreach (CharacterDefWithRole<CharacterRole_Jarl> jarl in CharacterRoleUtils.GetAllWithRole<CharacterRole_Jarl>())
            {
                if (jarl.characterDef.faction != def.rebelFaction || !tracker.CharacterPawnExistsAlive(jarl.characterDef))
                    continue;
                Pawn pawn = tracker.GetOrGenPawn(jarl.characterDef);
                if (pawn.Faction == loyal)
                    pawn.SetFaction(rebel);
            }

            int goodwill = loyal.GoodwillWith(Faction.OfPlayer) - rebel.GoodwillWith(Faction.OfPlayer);
            rebel.TryAffectGoodwillWith(Faction.OfPlayer, goodwill, false, false);

            if (def.slainCharacter != null)
            {
                Pawn slain = tracker.GetOrGenPawn(def.slainCharacter);
                if (!slain.Dead && slain.Faction == loyal)
                    slain.Kill(null);
            }

            Find.LetterStack.ReceiveLetter(def.letterLabel, def.letterText, LetterDefOf.NegativeEvent);
        }

        private static void TransferMap(Map map, Faction loyal, Faction rebel)
        {
            foreach (Thing thing in map.listerThings.AllThings.Where(t => t.Faction == loyal).ToList())
                thing.SetFaction(rebel);

            List<Pawn> pawns = map.mapPawns.SpawnedPawnsInFaction(rebel).ToList();
            if (pawns.Count > 0)
                LordMaker.MakeNewLord(rebel, new LordJob_DefendBase(rebel, map.Center, DefendBaseDelay), map, pawns);
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref resolved, "resolved", LookMode.Def);
            Scribe_Collections.Look(ref sideChoiceTicks, "sideChoiceTicks", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref sides, "sides", LookMode.Def, LookMode.Def);
            Scribe_Collections.Look(ref disabled, "disabled", LookMode.Def);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                resolved ??= new List<FactionSplitDef>();
                sideChoiceTicks ??= new Dictionary<FactionSplitDef, int>();
                sides ??= new Dictionary<FactionSplitDef, FactionDef>();
                disabled ??= new List<FactionSplitDef>();
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryExecuteWorker")]
    public static class IncidentWorker_RaidEnemy_Reinforcements_Patch
    {
        public static void Postfix(IncidentParms parms, bool __result)
        {
            if (__result && parms.faction != null && parms.target is Map map && map.IsPlayerHome)
                FactionSplitTracker.Instance.Notify_RaidedBy(parms.faction, map, parms.points);
        }
    }
}
