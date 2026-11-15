using RimWorld;
using Verse;

namespace ESCP.Buildings
{
    public class ThingProducerItem
    {
        public ThingDef thingDef;
        public ThingDef stuffDef;
        public IntRange countRange = IntRange.One;
        public float weight = 1f;
    }

    public class CompProperties_ThingProducer : CompProperties
    {
        public List<ThingProducerItem> items;
        public bool random = true;
        public int timeRequired = 300000;
        public bool displayMessage = true;
        public bool requireFuel;
        public bool requirePower;
        public bool onlyUnroofed;
        public bool restAtNight;
        public bool inTempRange;
        public FloatRange viableTempRange = new FloatRange(10f, 35f);
        public List<WeatherDef> disabledWeathers;
        public bool invertWeathers;

        public CompProperties_ThingProducer()
        {
            compClass = typeof(CompThingProducer);
        }
    }

    public class CompThingProducer : ThingComp
    {
        private int progressTicks;

        public CompProperties_ThingProducer Props => (CompProperties_ThingProducer)props;

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref progressTicks, "progressTicks");
        }

        public override void CompTick() => Progress(1);

        public override void CompTickRare() => Progress(GenTicks.TickRareInterval);

        public override void CompTickLong() => Progress(GenTicks.TickLongInterval);

        private void Progress(int ticks)
        {
            if (PauseReason() != null)
                return;

            progressTicks += ticks;
            if (progressTicks >= Props.timeRequired)
            {
                progressTicks = 0;
                Produce();
            }
        }

        private string PauseReason()
        {
            if (!parent.Spawned)
                return "";
            if (Props.onlyUnroofed && parent.Position.Roofed(parent.Map))
                return "ESCP_ThingProducer_Roofed".Translate();
            if (Props.restAtNight)
            {
                float day = GenLocalDate.DayPercent(parent);
                if (day < 0.25f || day > 0.8f)
                    return "ESCP_ThingProducer_Night".Translate();
            }
            if (Props.inTempRange && !Props.viableTempRange.Includes(parent.AmbientTemperature))
                return "ESCP_ThingProducer_Temperature".Translate();
            if (Props.disabledWeathers != null && Props.disabledWeathers.Contains(parent.Map.weatherManager.curWeather) != Props.invertWeathers)
                return "ESCP_ThingProducer_Weather".Translate();
            if (Props.requireFuel && parent.TryGetComp<CompRefuelable>() is { HasFuel: false })
                return "ESCP_ThingProducer_Fuel".Translate();
            if (Props.requirePower && parent.TryGetComp<CompPowerTrader>() is { PowerOn: false })
                return "ESCP_ThingProducer_Power".Translate();
            return null;
        }

        private void Produce()
        {
            if (!Props.items.NullOrEmpty())
            {
                if (Props.random)
                    Spawn(Props.items.RandomElementByWeight(i => i.weight));
                else
                    Props.items.ForEach(Spawn);
            }
            if (Props.displayMessage)
                Messages.Message("ESCP_ThingProducer_Produced".Translate(parent.LabelShort), parent, MessageTypeDefOf.PositiveEvent, false);
        }

        private void Spawn(ThingProducerItem item)
        {
            Thing thing = ThingMaker.MakeThing(item.thingDef, item.stuffDef);
            thing.stackCount = item.countRange.RandomInRange;
            GenPlace.TryPlaceThing(thing, parent.Position, parent.Map, ThingPlaceMode.Near);
        }

        public override string CompInspectStringExtra()
        {
            string reason = PauseReason();
            if (reason == null)
                return "ESCP_ThingProducer_Progress".Translate(((float)progressTicks / Props.timeRequired).ToStringPercent());
            return reason.Length == 0 ? null : "ESCP_ThingProducer_Paused".Translate(reason);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!DebugSettings.ShowDevGizmos)
                yield break;

            yield return new Command_Action
            {
                defaultLabel = "DEV: Finish production",
                action = () => progressTicks = Props.timeRequired
            };
        }
    }
}
