using System.Collections.Generic;
using Verse;

namespace ESCP.Guilds
{
    public class FactionGuildBranches : DefModExtension
    {
        public List<GuildType> guilds = new List<GuildType>();

        public bool HasGuild(GuildType guild) => guilds.Contains(guild);
    }
}
