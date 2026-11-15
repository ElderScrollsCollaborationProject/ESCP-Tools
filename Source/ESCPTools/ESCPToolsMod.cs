using ESCP.Enlist;
using ESCP.Factions;
using ESCP.Guilds;
using ESCP_Birthsigns;
using ESCP_RaceTools;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace ESCP.Core
{
    public class ESCPToolsMod : Mod
    {
        public static readonly HarmonyLib.Harmony Harmony = new HarmonyLib.Harmony("ESCP.Core.Patches");

        public static ESCPSettings Settings { get; private set; }

        public const string CurrencyPatchesCategory = "ESCP.Core.Currency";
        public const string LatePatchesCategory = "ESCP.Core.LatePatches";
        public const string ThrallPatchesCategory = "ESCP.Core.Thralls";

        private static Vector2 scrollPosition;
        private static int page;
        private static string calendarYearBuffer;

        public ESCPToolsMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ESCPSettings>();
            BackCompatibilityConverter_ESCP.Register();
            Harmony.PatchAllUncategorized();
            Harmony.PatchCategory(CurrencyPatchesCategory);
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                Harmony.PatchCategory(LatePatchesCategory);
                if (DefDatabase<HediffDef>.GetNamedSilentFail("ESCP_NecromanticThralls_Enthralled") != null)
                    Harmony.PatchCategory(ThrallPatchesCategory);
            });
        }
        public override string SettingsCategory() => "ESCP_Settings_Category".Translate();

        public override void WriteSettings()
        {
            base.WriteSettings();
            LoadedModManager.GetMod<ESCP_RaceTools_Mod>().WriteSettings();
            LoadedModManager.GetMod<BirthSigns_Mod>().WriteSettings();
            Settings.OnSaved?.Invoke();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            inRect.yMin += TabDrawer.TabHeight;
            Widgets.DrawMenuSection(inRect);
            TabDrawer.DrawTabs(inRect, new List<TabRecord>
            {
                new TabRecord("ESCP_Settings_Tab_Options".Translate(), () => page = 0, page == 0),
                new TabRecord("ESCP_Settings_Tab_Enlistment".Translate(), () => page = 1, page == 1),
                new TabRecord("ESCP_Settings_Tab_Races".Translate(), () => page = 2, page == 2),
                new TabRecord("ESCP_Settings_Tab_Birthsigns".Translate(), () => page = 3, page == 3)
            });

            Rect rect = inRect.ContractedBy(10f);
            switch (page)
            {
                case 0:
                    DrawOptions(rect);
                    break;
                case 1:
                    DrawEnlistment(rect);
                    break;
                case 2:
                    LoadedModManager.GetMod<ESCP_RaceTools_Mod>().DoSettingsWindowContents(rect);
                    break;
                default:
                    LoadedModManager.GetMod<BirthSigns_Mod>().DoSettingsWindowContents(rect);
                    break;
            }
        }

        private static void DrawOptions(Rect rect)
        {
            var defs = DefDatabase<ModOptionDef>.AllDefsListForReading;
            var viewRect = new Rect(0f, 0f, rect.width - 30f, 150f + defs.Count * 60f);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);

            var listing = new Listing_Standard();
            listing.Begin(viewRect);
            bool inGame = Current.ProgramState == ProgramState.Playing;
            int era = inGame ? EraTracker.Era : Settings.characterEra;
            foreach (ModOptionDef def in defs)
            {
                bool allowed = DefDatabase<FactionSplitDef>.AllDefsListForReading.All(s => s.option != def || s.AllowsEra(era));
                bool value = allowed && def.Enabled;
                Widgets.CheckboxLabeled(listing.GetRect(Text.LineHeight), def.LabelCap, ref value, !allowed || inGame && def.worldGenOnly);
                if (allowed && (!inGame || !def.worldGenOnly))
                    def.Enabled = value;
                listing.Label(def.description);
                listing.Gap(6f);
            }

            string eraLabel = era == 0 ? "ESCP_Settings_CharacterEraAll".Translate() : "ESCP_Settings_CalendarEraValue".Translate(era);
            if (listing.ButtonTextLabeled("ESCP_Settings_CharacterEra".Translate(), eraLabel, tooltip: "ESCP_Settings_CharacterEraDesc".Translate()) && !inGame)
                Settings.characterEra = (Settings.characterEra + 1) % 5;

            if (inGame || Settings.characterEra != 0)
            {
                int startEra = inGame ? EraTracker.CalendarEra : Settings.StartEra;
                int startYear = inGame ? EraTracker.CalendarYear : Settings.StartYear;
                listing.Label("ESCP_Settings_CalendarStart".Translate(startEra, startYear));
            }
            else
            {
                if (listing.ButtonTextLabeled("ESCP_Settings_CalendarEra".Translate(), "ESCP_Settings_CalendarEraValue".Translate(Settings.calendarEra)))
                    Settings.calendarEra = Settings.calendarEra % 4 + 1;
                listing.TextFieldNumericLabeled("ESCP_Settings_CalendarYear".Translate(), ref Settings.calendarYear, ref calendarYearBuffer, 1, 9999);
            }
            if (inGame)
                listing.Label("ESCP_Settings_WorldGenOnly".Translate());
            listing.End();
            Widgets.EndScrollView();
        }

        private static void DrawEnlistment(Rect rect)
        {
            if (enlistGuilds == null)
                BuildEnlistLists();

            var viewRect = new Rect(0f, 0f, rect.width - 30f, enlistHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);

            var listing = new Listing_Standard();
            listing.Begin(viewRect);
            listing.Label("ESCP_Settings_Enlistment_Intro".Translate());
            listing.Gap();
            DrawEnlistGroup(listing, "ESCP_Settings_Enlistment_Guilds", enlistGuilds);
            listing.Gap();
            DrawEnlistGroup(listing, "ESCP_Settings_Enlistment_Factions", enlistFactions);
            enlistHeight = listing.CurHeight + 20f;
            listing.End();
            Widgets.EndScrollView();
        }

        private static List<FactionEnlistOptionsDef> enlistGuilds;
        private static List<FactionEnlistOptionsDef> enlistFactions;
        private static Dictionary<FactionEnlistOptionsDef, string> enlistTips;
        private static float enlistHeight = 1200f;

        private static void BuildEnlistLists()
        {
            var guildDefs = new HashSet<FactionEnlistOptionsDef>();
            foreach (GuildBranchDef branch in DefDatabase<GuildBranchDef>.AllDefsListForReading)
            {
                var options = DefDatabase<FactionDef>.GetNamed(branch.GuildFactionDefName, false)?.GetModExtension<FactionEnlistOptions>();
                if (options?.enlistOptionsDefs != null)
                    guildDefs.AddRange(options.enlistOptionsDefs);
            }

            var all = DefDatabase<FactionEnlistOptionsDef>.AllDefs.Where(d => Settings.enlistStates.ContainsKey(d.defName)).OrderBy(d => d.label).ToList();
            enlistGuilds = all.Where(guildDefs.Contains).ToList();
            enlistFactions = all.Where(d => !guildDefs.Contains(d)).ToList();
            enlistTips = new Dictionary<FactionEnlistOptionsDef, string>();
            foreach (FactionEnlistOptionsDef def in all)
            {
                var lines = new List<string>();
                if (!def.enlistButtonDescKey.NullOrEmpty())
                    lines.Add(def.enlistButtonDescKey.Translate() + "\n");
                lines.Add(def.salaryIsEnabled
                    ? "ESCP_Settings_Enlistment_Pay".Translate(Mathf.RoundToInt(def.salaryRange.min), Mathf.RoundToInt(def.salaryRange.max), def.salaryDef?.label ?? "", def.salaryPeriodDays).ToString()
                    : "ESCP_Settings_Enlistment_NoPay".Translate().ToString());
                if (!def.provisionOptions.NullOrEmpty())
                    lines.Add("ESCP_Settings_Enlistment_Provisions".Translate(Mathf.RoundToInt(def.provisionOptions[0].provisionsRestockInDays)));
                if (def.storageIsEnabled)
                    lines.Add("ESCP_Settings_Enlistment_Storage".Translate());
                if (!def.workOptions.NullOrEmpty())
                    lines.Add("ESCP_Settings_Enlistment_Work".Translate(def.workOptions.Select(w => w.workLabelKey.Translate().ToString()).ToCommaList()));
                if (def.missionsAreEnabled)
                    lines.Add("ESCP_Settings_Enlistment_Missions".Translate());
                enlistTips[def] = string.Join("\n", lines);
            }
        }

        private static void DrawEnlistGroup(Listing_Standard listing, string key, List<FactionEnlistOptionsDef> defs)
        {
            listing.Label(key.Translate());
            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            listing.Label((key + "_Desc").Translate());
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            listing.GapLine();
            foreach (FactionEnlistOptionsDef def in defs)
            {
                bool value = Settings.enlistStates[def.defName];
                listing.CheckboxLabeled(def.LabelCap, ref value, enlistTips[def]);
                Settings.enlistStates[def.defName] = value;
            }
        }
    }
}
