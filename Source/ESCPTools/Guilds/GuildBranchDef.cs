using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ESCP.Guilds
{
    public class GuildBranchDef : Def
    {
        public GuildType    guild;
        public List<string> hostFactionDefs = new List<string>();
        public string       menuIconPath;
        public string       guildActionLabel;
        public string       guildFactionDef;

        public string GuildFactionDefName
        {
            get
            {
                if (!guildFactionDef.NullOrEmpty())
                    return guildFactionDef;

                switch (guild)
                {
                    case GuildType.FightersGuild:
                    case GuildType.Companions:
                        return "ESCP_Faction_FightersGuild";
                    case GuildType.MagesGuild:
                    case GuildType.CollegeOfWinterhold:
                        return "ESCP_Faction_MagesGuild";
                    case GuildType.ThievesGuild:
                        return "ESCP_Faction_ThievesGuild";
                    default:
                        return "ESCP_Faction_DarkBrotherhood";
                }
            }
        }

        public Texture2D MenuIcon =>
            menuIconPath.NullOrEmpty() ? null : ContentFinder<Texture2D>.Get(menuIconPath, true);
    }
}
