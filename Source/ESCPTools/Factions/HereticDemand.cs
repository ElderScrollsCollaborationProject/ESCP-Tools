using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace ESCP.Factions
{
    public class HereticDemandExtension : DefModExtension
    {
        public FactionDef faction;
        public List<MemeDef> memes = new List<MemeDef>();
        public int handOverGoodwill = 15;
        public IntRange raidDelayTicks = new IntRange(20000, 40000);
        public int timeoutTicks = 60000;
        public QuestScriptDef rescueQuest;
        [MustTranslate] public string handOverLabel;
        [MustTranslate] public string handOverText;
        [MustTranslate] public string refuseLabel;
        [MustTranslate] public string refuseText;
    }

    public class IncidentWorker_HereticDemand : IncidentWorker
    {
        private HereticDemandExtension Props => def.GetModExtension<HereticDemandExtension>();

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return ModsConfig.IdeologyActive && DemandingFaction() is Faction faction && !faction.HostileTo(Faction.OfPlayer)
                && TryFindHeretic((Map)parms.target, out _);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            Faction faction = DemandingFaction();
            if (faction == null || !TryFindHeretic(map, out Pawn heretic))
                return false;

            ChoiceLetter_HereticDemand letter = (ChoiceLetter_HereticDemand)LetterMaker.MakeLetter(def.letterLabel.Formatted(heretic.Named("PAWN")), def.letterText.Formatted(heretic.Named("PAWN"), faction.Named("FACTION")), def.letterDef, heretic, faction);
            letter.map = map;
            letter.faction = faction;
            letter.heretic = heretic;
            letter.incident = def;
            letter.StartTimeout(Props.timeoutTicks);
            Find.LetterStack.ReceiveLetter(letter);
            return true;
        }

        private Faction DemandingFaction()
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(Props.faction);
            return faction == null || faction.defeated ? null : faction;
        }

        private bool TryFindHeretic(Map map, out Pawn heretic)
        {
            List<MemeDef> memes = Props.memes;
            return map.mapPawns.FreeColonistsSpawned
                .Where(p => !p.IsQuestLodger() && p.Ideo != null && memes.Any(p.Ideo.HasMeme))
                .TryRandomElement(out heretic);
        }
    }

    public class ChoiceLetter_HereticDemand : ChoiceLetter
    {
        public Map map;
        public Faction faction;
        public Pawn heretic;
        public IncidentDef incident;
        private bool answered;

        private HereticDemandExtension Props => incident.GetModExtension<HereticDemandExtension>();

        public override bool CanDismissWithRightClick => false;

        public override bool CanShowInLetterStack => base.CanShowInLetterStack && heretic.MapHeld == map && !heretic.Dead && heretic.Faction == Faction.OfPlayer;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly)
                {
                    yield return Option_Close;
                    yield break;
                }
                yield return new DiaOption(Props.handOverLabel.Formatted(heretic.Named("PAWN")))
                {
                    action = HandOver,
                    resolveTree = true
                };
                yield return new DiaOption(Props.refuseLabel)
                {
                    action = () =>
                    {
                        answered = true;
                        Refuse();
                        Find.LetterStack.RemoveLetter(this);
                    },
                    resolveTree = true
                };
                yield return Option_Postpone;
            }
        }

        private void HandOver()
        {
            answered = true;
            Find.LetterStack.RemoveLetter(this);
            faction.kidnapped.Kidnap(heretic, null);
            faction.TryAffectGoodwillWith(Faction.OfPlayer, Props.handOverGoodwill, false);
            Find.LetterStack.ReceiveLetter(incident.letterLabel.Formatted(heretic.Named("PAWN")), Props.handOverText.Formatted(heretic.Named("PAWN"), faction.Named("FACTION")), LetterDefOf.NegativeEvent);

            if (Props.rescueQuest == null)
                return;
            Slate slate = new Slate();
            slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(map));
            slate.Set("captive", heretic);
            slate.Set("captors", faction);
            if (Props.rescueQuest.CanRun(slate, map))
                QuestUtility.SendLetterQuestAvailable(QuestUtility.GenerateQuestAndMakeAvailable(Props.rescueQuest, slate));
        }

        private void Refuse()
        {
            faction.TryAffectGoodwillWith(Faction.OfPlayer, -faction.GoodwillWith(Faction.OfPlayer) - 100, false, false);
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
            parms.faction = faction;
            parms.forced = true;
            Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, Find.TickManager.TicksGame + Props.raidDelayTicks.RandomInRange, parms);
            Find.LetterStack.ReceiveLetter(incident.letterLabel.Formatted(heretic.Named("PAWN")), Props.refuseText.Formatted(heretic.Named("PAWN"), faction.Named("FACTION")), LetterDefOf.ThreatBig, heretic);
        }

        public override void Removed()
        {
            base.Removed();
            if (!answered && TimeoutPassed && !heretic.Dead && Find.Maps.Contains(map))
            {
                answered = true;
                Refuse();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref map, "map");
            Scribe_References.Look(ref faction, "faction");
            Scribe_References.Look(ref heretic, "heretic");
            Scribe_Defs.Look(ref incident, "incident");
            Scribe_Values.Look(ref answered, "answered");
        }
    }
}
