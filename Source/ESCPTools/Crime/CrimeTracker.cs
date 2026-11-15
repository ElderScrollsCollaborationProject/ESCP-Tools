using ESCP.Guilds;
using HarmonyLib;
using RimWorld.Planet;

namespace ESCP.Core.Crime;

public class FactionExtension_Law : DefModExtension
{
}

public class JailSentence : IExposable
{
    public Pawn pawn;
    public int releaseTick;

    public void ExposeData()
    {
        Scribe_References.Look(ref pawn, "pawn", saveDestroyedThings: true);
        Scribe_Values.Look(ref releaseTick, "releaseTick");
    }
}

public class CrimeTracker : WorldComponent
{
    private const int CheckInterval = 2500;

    private Dictionary<Faction, int> bounties = new Dictionary<Faction, int>();
    private List<JailSentence> sentences = new List<JailSentence>();
    private List<Faction> factionKeys;
    private List<int> bountyValues;

    public CrimeTracker(World world) : base(world) { }

    public static CrimeTracker Current => Find.World?.GetComponent<CrimeTracker>();

    public IEnumerable<KeyValuePair<Faction, int>> Bounties => bounties;

    public int BountyFor(Faction faction)
    {
        return faction != null && bounties.TryGetValue(faction, out int value) ? value : 0;
    }

    public void AddBounty(Faction faction, int amount)
    {
        if (faction?.def.GetModExtension<FactionExtension_Law>() != null && amount > 0)
            bounties[faction] = BountyFor(faction) + amount;
    }

    public void ClearBounty(Faction faction)
    {
        bounties.Remove(faction);
    }

    public void Sentence(Pawn pawn, int ticks)
    {
        sentences.Add(new JailSentence { pawn = pawn, releaseTick = Find.TickManager.TicksGame + ticks });
    }

    public override void WorldComponentTick()
    {
        if (sentences.Count == 0 || Find.TickManager.TicksGame % CheckInterval != 0)
            return;

        Map map = Find.AnyPlayerHomeMap;
        if (map == null)
            return;

        for (int i = sentences.Count - 1; i >= 0; i--)
        {
            JailSentence sentence = sentences[i];
            if (sentence.pawn == null || sentence.pawn.Dead)
            {
                sentences.RemoveAt(i);
            }
            else if (Find.TickManager.TicksGame >= sentence.releaseTick)
            {
                CrimeUtility.Release(sentence.pawn, map);
                sentences.RemoveAt(i);
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref bounties, "bounties", LookMode.Reference, LookMode.Value, ref factionKeys, ref bountyValues);
        Scribe_Collections.Look(ref sentences, "sentences", LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            bounties ??= new Dictionary<Faction, int>();
            sentences ??= new List<JailSentence>();
            bounties.RemoveAll(pair => pair.Key == null);
        }
    }
}

public static class CrimeUtility
{
    public const int GuardThreshold = 200;
    public const float BountyPerGoodwill = 10f;
    private const int JailTicksPerSilver = 60;
    private const int MaxJailTicks = GenDate.TicksPerDay * 15;
    private const int ThievesDiscountRep = 20;
    private const int MissionBounty = 80;
    private const float MissionWitnessChance = 0.3f;

    private static readonly HashSet<string> CrimeReasons = new HashSet<string>
    {
        "AttackedMember", "AttackedBuilding", "AttackedSettlement", "AttackedCaravan",
        "MemberKilled", "MemberCaptured", "MemberSold", "MemberStripped",
        "InnocentPrisonerDied", "PrisonerDied", "UsedHarmfulAbility", "UsedHarmfulItem", "PerformedHarmfulSurgery"
    };

    public static bool IsCrime(HistoryEventDef reason)
    {
        return reason != null && CrimeReasons.Contains(reason.defName);
    }

    public static int FineFor(Faction faction)
    {
        int bounty = CrimeTracker.Current?.BountyFor(faction) ?? 0;
        return GuildTracker.Current?.GetRep(GuildType.ThievesGuild) >= ThievesDiscountRep ? bounty / 2 : bounty;
    }

    public static int JailTicksFor(Faction faction)
    {
        return Math.Min((CrimeTracker.Current?.BountyFor(faction) ?? 0) * JailTicksPerSilver, MaxJailTicks);
    }

    public static int SilverOn(Map map)
    {
        return map.listerThings.ThingsOfDef(ThingDefOf.Silver).Sum(t => t.stackCount);
    }

    public static bool PayFine(Map map, Faction faction)
    {
        int fine = FineFor(faction);
        List<Thing> silver = map.listerThings.ThingsOfDef(ThingDefOf.Silver);
        if (SilverOn(map) < fine)
            return false;

        int remaining = fine;
        for (int i = silver.Count - 1; i >= 0 && remaining > 0; i--)
        {
            int take = Math.Min(remaining, silver[i].stackCount);
            silver[i].SplitOff(take).Destroy();
            remaining -= take;
        }

        CrimeTracker.Current.ClearBounty(faction);
        Messages.Message("ESCP_Crime_Paid".Translate(fine, faction.Name), MessageTypeDefOf.PositiveEvent);
        return true;
    }

    public static bool PayFine(Caravan caravan, Faction faction)
    {
        int remaining = FineFor(faction);
        List<Thing> silver = CaravanInventoryUtility.AllInventoryItems(caravan).Where(t => t.def == ThingDefOf.Silver).ToList();
        if (silver.Sum(t => t.stackCount) < remaining)
            return false;

        int fine = remaining;
        foreach (Thing stack in silver)
        {
            int take = Math.Min(remaining, stack.stackCount);
            stack.SplitOff(take).Destroy();
            remaining -= take;
            if (remaining <= 0)
                break;
        }

        CrimeTracker.Current.ClearBounty(faction);
        Messages.Message("ESCP_Crime_Paid".Translate(fine, faction.Name), caravan, MessageTypeDefOf.PositiveEvent);
        return true;
    }

    public static void ServeTime(Pawn pawn, Faction faction)
    {
        int ticks = JailTicksFor(faction);
        pawn.DeSpawn();
        Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.KeepForever);
        CrimeTracker.Current.Sentence(pawn, ticks);
        CrimeTracker.Current.ClearBounty(faction);
        Messages.Message("ESCP_Crime_Jailed".Translate(pawn.LabelShort, faction.Name, ticks.ToStringTicksToDays()), MessageTypeDefOf.NeutralEvent);
    }

    public static void Release(Pawn pawn, Map map)
    {
        if (Find.WorldPawns.Contains(pawn))
            Find.WorldPawns.RemovePawn(pawn);
        DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(map), map, new List<Thing> { pawn }, canRoofPunch: false);
        pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(CrimeDefOf.ESCP_Thought_ServedTime);
        Messages.Message("ESCP_Crime_Released".Translate(pawn.LabelShort), pawn, MessageTypeDefOf.PositiveEvent);
    }

    public static void Resist(Map map, Faction faction)
    {
        faction.TryAffectGoodwillWith(Faction.OfPlayer, -200, reason: HistoryEventDefOf.AttackedMember);
        IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
        parms.faction = faction;
        parms.forced = true;
        IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
    }

    public static void Notify_ShadyContract(GuildType guild)
    {
        if ((guild != GuildType.ThievesGuild && guild != GuildType.DarkBrotherhood) || !Rand.Chance(MissionWitnessChance))
            return;

        Faction witness = Find.FactionManager.AllFactionsVisible.Where(f => f.def.GetModExtension<FactionExtension_Law>() != null && !f.HostileTo(Faction.OfPlayer)).RandomElementWithFallback();
        if (witness == null)
            return;

        CrimeTracker.Current?.AddBounty(witness, MissionBounty);
        Messages.Message("ESCP_Crime_Witnessed".Translate(witness.Name, MissionBounty), MessageTypeDefOf.NegativeEvent);
    }
}

[HarmonyPatch(typeof(Faction), nameof(Faction.TryAffectGoodwillWith))]
public static class Faction_TryAffectGoodwillWith_Crime_Patch
{
    public static void Postfix(Faction __instance, Faction other, int goodwillChange, HistoryEventDef reason, bool __result)
    {
        if (__result && goodwillChange < 0 && other == Faction.OfPlayer && CrimeUtility.IsCrime(reason))
            CrimeTracker.Current?.AddBounty(__instance, (int)(-goodwillChange * CrimeUtility.BountyPerGoodwill));
    }
}

public class Alert_Bounty : Alert
{
    public Alert_Bounty()
    {
        defaultLabel = "ESCP_Alert_Bounty_Label".Translate();
        defaultPriority = AlertPriority.Medium;
    }

    public override TaggedString GetExplanation()
    {
        return "ESCP_Alert_Bounty_Desc".Translate(CrimeTracker.Current.Bounties.Select(pair => pair.Key.Name + ": " + pair.Value).ToLineList("  - "));
    }

    public override AlertReport GetReport()
    {
        return CrimeTracker.Current?.Bounties.Any() == true;
    }
}

public class IncidentWorker_GuardsConfront : IncidentWorker
{
    private static Faction Target => CrimeTracker.Current?.Bounties
        .Where(pair => pair.Value >= CrimeUtility.GuardThreshold && !pair.Key.HostileTo(Faction.OfPlayer))
        .OrderByDescending(pair => pair.Value)
        .Select(pair => pair.Key)
        .FirstOrDefault();

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        return Target != null;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Faction faction = Target;
        Map map = (Map)parms.target;
        if (faction == null)
            return false;

        int fine = CrimeUtility.FineFor(faction);
        Pawn convict = map.mapPawns.FreeColonistsSpawned.Where(p => !p.Downed).RandomElementWithFallback();
        DiaNode node = new DiaNode("ESCP_Crime_Demand".Translate(faction.Name, fine));

        DiaOption pay = new DiaOption("ESCP_Crime_OptionPay".Translate(fine)) { resolveTree = true, action = () => CrimeUtility.PayFine(map, faction) };
        if (CrimeUtility.SilverOn(map) < fine)
            pay.Disable("ESCP_Crime_NeedSilver".Translate(fine));
        node.options.Add(pay);

        DiaOption jail = new DiaOption("ESCP_Crime_OptionJail".Translate(convict?.LabelShort ?? "?", CrimeUtility.JailTicksFor(faction).ToStringTicksToDays()))
        {
            resolveTree = true,
            action = () => CrimeUtility.ServeTime(convict, faction)
        };
        if (convict == null)
            jail.Disable(null);
        node.options.Add(jail);

        node.options.Add(new DiaOption("ESCP_Crime_OptionResist".Translate()) { resolveTree = true, action = () => CrimeUtility.Resist(map, faction) });

        Find.WindowStack.Add(new Dialog_NodeTree(node, delayInteractivity: true, radioMode: true, "ESCP_Crime_DemandTitle".Translate()));
        return true;
    }
}

public class WorldObjectCompProperties_Bounty : WorldObjectCompProperties
{
    public WorldObjectCompProperties_Bounty()
    {
        compClass = typeof(WorldObjectComp_Bounty);
    }
}

public class WorldObjectComp_Bounty : WorldObjectComp
{
    public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
    {
        Faction faction = parent.Faction;
        if (faction == null || CrimeTracker.Current?.BountyFor(faction) <= 0)
            yield break;

        int fine = CrimeUtility.FineFor(faction);
        Command_Action command = new Command_Action
        {
            defaultLabel = "ESCP_Crime_PayCommand".Translate(fine),
            defaultDesc = "ESCP_Crime_PayCommandDesc".Translate(faction.Name),
            icon = ThingDefOf.Silver.uiIcon,
            action = () => CrimeUtility.PayFine(caravan, faction)
        };
        if (CaravanInventoryUtility.AllInventoryItems(caravan).Where(t => t.def == ThingDefOf.Silver).Sum(t => t.stackCount) < fine)
            command.Disable("ESCP_Crime_NeedSilver".Translate(fine));
        yield return command;
    }
}

[HarmonyPatch(typeof(Settlement_TraderTracker), nameof(Settlement_TraderTracker.CanTradeNow), MethodType.Getter)]
public static class Settlement_TraderTracker_CanTradeNow_Patch
{
    public static void Postfix(Settlement_TraderTracker __instance, ref bool __result)
    {
        if (__result && CrimeTracker.Current?.BountyFor(__instance.settlement.Faction) >= CrimeUtility.GuardThreshold)
            __result = false;
    }
}

[DefOf]
public static class CrimeDefOf
{
    public static ThoughtDef ESCP_Thought_ServedTime;

    static CrimeDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(CrimeDefOf));
    }
}
