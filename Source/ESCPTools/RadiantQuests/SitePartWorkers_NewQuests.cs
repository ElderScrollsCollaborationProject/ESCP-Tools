using RimWorld.Planet;

namespace ESCP.Core.RadiantQuests;

public class SitePartWorker_RelicCache : SitePartWorker
{
    public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
    {
        return base.GetPostProcessedThreatLabel(site, sitePart) + ": " + "ESCP_RelicCacheLabel".Translate();
    }

    public override SitePartParams GenerateDefaultParams(float myThreatPoints, PlanetTile tile, Faction faction)
    {
        SitePartParams parms = base.GenerateDefaultParams(myThreatPoints, tile, faction);
        parms.threatPoints = myThreatPoints;
        return parms;
    }
}
