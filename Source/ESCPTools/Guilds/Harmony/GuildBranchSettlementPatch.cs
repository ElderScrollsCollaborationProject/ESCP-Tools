using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace ESCP.Guilds
{
    [HarmonyPatch(typeof(Settlement), "GetCaravanGizmos", typeof(Caravan))]
    public static class Settlement_GetCaravanGizmos_GuildBranchPatch
    {
        [HarmonyPostfix]
        public static IEnumerable<Gizmo> Postfix(
            IEnumerable<Gizmo> __result,
            Settlement __instance,
            Caravan caravan)
        {
            return __result.Concat(GuildGizmos(__instance, caravan));
        }

        private static IEnumerable<Gizmo> GuildGizmos(Settlement settlement, Caravan caravan)
        {
            if (settlement.Faction == null || caravan == null)
                yield break;

            string hostDefName = settlement.Faction.def.defName;

            foreach (var branchDef in DefDatabase<GuildBranchDef>.AllDefs)
            {
                if (!branchDef.hostFactionDefs.Contains(hostDefName))
                    continue;

                string guildFactionDefName = branchDef.GuildFactionDefName;

                Faction guildFaction = Find.FactionManager.AllFactions
                    .FirstOrDefault(f => f.def.defName == guildFactionDefName);

                if (guildFaction == null || guildFaction.HostileTo(Faction.OfPlayer))
                    continue;

                var questScripts = GuildQuestScripts(branchDef.guild);
                if (!questScripts.Any())
                    continue;

                string label = !branchDef.guildActionLabel.NullOrEmpty()
                    ? branchDef.guildActionLabel
                    : branchDef.guild == GuildType.FightersGuild
                        ? "ESCP_RequestFightersGuildContract".Translate()
                        : branchDef.guild == GuildType.ThievesGuild
                            ? "ESCP_RequestThievesGuildJob".Translate()
                            : branchDef.guild == GuildType.DarkBrotherhood
                                ? "ESCP_RequestDarkBrotherhoodContract".Translate()
                                : (string)"ESCP_RequestMagesGuildCommission".Translate();

                var capturedGuild      = branchDef.guild;
                var capturedFaction    = guildFaction;
                var capturedSettlement = settlement;

                var cmd = new Command_Action
                {
                    defaultLabel = label,
                    defaultDesc  = label,
                    action       = () => RequestGuildQuest(capturedGuild, capturedFaction, capturedSettlement)
                };

                var icon = branchDef.MenuIcon;
                if (icon != null)
                    cmd.icon = icon;

                yield return cmd;
            }
        }

        private static List<QuestScriptDef> GuildQuestScripts(GuildType guild)
        {
            bool isFighterType = guild == GuildType.FightersGuild || guild == GuildType.Companions;
            string prefix = isFighterType
                ? "ESCP_Quest_FightersGuild_"
                : guild == GuildType.MagesGuild || guild == GuildType.CollegeOfWinterhold
                    ? "ESCP_Quest_MagesGuild_"
                    : guild == GuildType.ThievesGuild
                        ? "ESCP_Quest_ThievesGuild_"
                        : "ESCP_Quest_DarkBrotherhood_";

            return DefDatabase<QuestScriptDef>.AllDefs
                .Where(d => d.defName.StartsWith(prefix) && d.rootSelectionWeight > 0f)
                .ToList();
        }

        private static void RequestGuildQuest(
            GuildType guild,
            Faction guildFaction,
            Settlement hostSettlement)
        {
            var scripts = GuildQuestScripts(guild);
            if (!scripts.Any())
            {
                Messages.Message(
                    "ESCP_GuildQuestUnavailable".Translate(guildFaction.Name),
                    MessageTypeDefOf.RejectInput,
                    historical: false);
                return;
            }

            float points = Mathf.Max(
                StorytellerUtility.DefaultThreatPointsNow(Find.World),
                scripts.Min(s => s.rootMinPoints));

            QuestScriptDef chosen = scripts
                .Where(s =>
                {
                    var ts = new Slate();
                    ts.Set("points", points);
                    return s.CanRun(ts, Find.World);
                })
                .RandomElementByWeightWithFallback(s => s.rootSelectionWeight);

            if (chosen == null)
            {
                Messages.Message(
                    "ESCP_GuildQuestUnavailable".Translate(guildFaction.Name),
                    MessageTypeDefOf.RejectInput,
                    historical: false);
                return;
            }

            Slate genSlate = new Slate();
            genSlate.Set("points", points);
            genSlate.Set("hostFaction", hostSettlement.Faction);

            Quest quest = QuestGen.Generate(chosen, genSlate);
            if (quest == null)
            {
                Messages.Message(
                    "ESCP_GuildQuestUnavailable".Translate(guildFaction.Name),
                    MessageTypeDefOf.RejectInput,
                    historical: false);
                return;
            }

            Find.QuestManager.Add(quest);
        }
    }
}