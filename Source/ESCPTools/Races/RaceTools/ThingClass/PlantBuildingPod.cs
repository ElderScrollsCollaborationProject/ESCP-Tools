using Verse;
using RimWorld;

namespace ESCP_RaceTools
{
    [StaticConstructorOnStartup]
    public class PlantBuildingPod : Plant
    {
		protected override void Tick()
		{
			base.Tick();
			if (this.IsHashIntervalTick(2000))
			{
				TickLong();
			}
		}

		public override void TickLong()
		{
			if (Destroyed)
			{
				return;
			}

			var modExt = PlantProperties.Get(def);
			if (modExt != null && modExt.matureInto != null  && Growth >= 1f)
            {
				IntVec3 loc = Position;
				Map map = Map;

				GenSpawn.Spawn(modExt.matureInto, loc, map);
				return;
			}

			base.TickLong();
		}
	}
}
