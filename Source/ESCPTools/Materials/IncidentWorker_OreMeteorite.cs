using RimWorld;
using Verse;

namespace ESCP.Materials
{
    public class IncidentExtension_OreMeteorite : DefModExtension
    {
        public ThingDef mineable;
    }

    public class IncidentWorker_OreMeteorite : IncidentWorker_MeteoriteImpact
    {
        protected override List<Thing> GenerateMeteorContents(IncidentParms parms)
        {
            ThingDef mineable = def.GetModExtension<IncidentExtension_OreMeteorite>().mineable;
            int count = ThingSetMaker_Meteorite.MineablesCountRange.RandomInRange;
            List<Thing> contents = new List<Thing>(count);
            for (int i = 0; i < count; i++)
                contents.Add(ThingMaker.MakeThing(mineable));
            return contents;
        }
    }
}
