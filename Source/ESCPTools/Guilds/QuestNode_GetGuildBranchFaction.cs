using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace ESCP.Guilds
{
    public class QuestNode_GetGuildBranchFaction : QuestNode
    {
        public GuildType           guild;
        public SlateRef<string>    storeAs      = "hostFaction";
        public TechLevel           maxTechLevel = TechLevel.Medieval;

        protected override bool TestRunInt(Slate slate)
        {
            Faction preSet;
            if (slate.TryGet(storeAs.GetValue(slate), out preSet) && preSet != null)
                return true;

            var host = FindBranchFaction() ?? FindGuildFaction();
            if (host == null)
                return false;

            slate.Set(storeAs.GetValue(slate), host);
            return true;
        }

        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            Faction preSet;
            if (slate.TryGet(storeAs.GetValue(slate), out preSet) && preSet != null)
                return;

            var host = FindBranchFaction() ?? FindGuildFaction();
            if (host != null)
                slate.Set(storeAs.GetValue(slate), host);
        }

        private Faction FindBranchFaction()
        {
            return Find.FactionManager.AllFactions
                .Where(f => !f.IsPlayer
                         && !f.defeated
                         && !f.HostileTo(Faction.OfPlayer)
                         && f.def.techLevel <= maxTechLevel)
                .Where(f => IsListedInBranchDef(f) || HasGuildExtension(f))
                .RandomElementWithFallback();
        }

        private bool IsListedInBranchDef(Faction f)
        {
            foreach (var def in DefDatabase<GuildBranchDef>.AllDefs)
            {
                if (def.guild == guild && def.hostFactionDefs.Contains(f.def.defName))
                    return true;
            }
            return false;
        }

        private bool HasGuildExtension(Faction f)
        {
            return f.def.GetModExtension<FactionGuildBranches>()?.HasGuild(guild) == true;
        }

        private Faction FindGuildFaction()
        {
            var defName = guild == GuildType.FightersGuild
                ? "ESCP_Faction_FightersGuild"
                : "ESCP_Faction_MagesGuild";

            return Find.FactionManager.AllFactions
                .FirstOrDefault(f => f.def.defName == defName);
        }
    }
}
