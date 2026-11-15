namespace ESCP.Ranks;

public class RankTrackDef : Def
{
    public string iconPath;
    public ESCP.Guilds.GuildType? guild;
    public RankTierDef rootTier;
    public FactionDef owningFaction;
    public string collectiveNoun = "the order";

    public string drillLabel;
    public string drillDesc;
    public string studyLabel;
    public string studyDesc;
    public string inspectLabel;
    public string inspectDesc;
    public string punishLabel;
    public string punishDesc;
    public string rallyLabel;
    public string rallyDesc;
    public string fallInLabel;
    public string fallInDesc;
    public string dismissLabel;
    public string dismissDesc;
}
