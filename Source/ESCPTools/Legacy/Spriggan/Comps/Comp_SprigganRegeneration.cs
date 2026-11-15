using Verse;
using RimWorld;

namespace ESCP_Spriggan
{
    class Comp_SprigganRegeneration : ThingComp
	{

		public CompProperties_SprigganRegeneration Props
		{
			get
			{
				return (CompProperties_SprigganRegeneration)this.props;
			}
		}

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            Pawn pawn = parent as Pawn;
            if (!respawningAfterLoad && pawn.Faction == null && !pawn.Downed && pawn.MentalStateDef == null)
                pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);

            Pawn pawn = parent as Pawn;
            if (!pawn.Dead && ESCP_Spriggan_ModSettings.EnableRegen && pawn.Spawned && pawn.health.hediffSet.PainTotal >= Props.minPain && !pawn.health.hediffSet.HasHediff(Props.hediff))
            {
                pawn.health.AddHediff(Props.hediff).Severity = Props.Severity;
            }
        }
    }
}