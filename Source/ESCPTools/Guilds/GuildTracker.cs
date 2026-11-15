using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace ESCP.Guilds
{
    public class GuildTracker : WorldComponent
    {
        private int fightersRep;
        private int magesRep;
        private int companionsRep;
        private int collegeRep;
        private int thievesRep;
        private int brotherhoodRep;

        public GuildTracker(World world) : base(world) { }

        public static GuildTracker Current => Find.World?.GetComponent<GuildTracker>();

        public int GetRep(GuildType guild)
        {
            switch (guild)
            {
                case GuildType.FightersGuild:       return fightersRep;
                case GuildType.MagesGuild:          return magesRep;
                case GuildType.Companions:          return companionsRep;
                case GuildType.CollegeOfWinterhold: return collegeRep;
                case GuildType.ThievesGuild:        return thievesRep;
                case GuildType.DarkBrotherhood:     return brotherhoodRep;
                default:                            return 0;
            }
        }

        public void AddRep(GuildType guild, int amount)
        {
            switch (guild)
            {
                case GuildType.FightersGuild:       fightersRep    = Mathf.Clamp(fightersRep    + amount, -100, 100); break;
                case GuildType.MagesGuild:          magesRep       = Mathf.Clamp(magesRep       + amount, -100, 100); break;
                case GuildType.Companions:          companionsRep  = Mathf.Clamp(companionsRep  + amount, -100, 100); break;
                case GuildType.CollegeOfWinterhold: collegeRep     = Mathf.Clamp(collegeRep     + amount, -100, 100); break;
                case GuildType.ThievesGuild:        thievesRep     = Mathf.Clamp(thievesRep     + amount, -100, 100); break;
                case GuildType.DarkBrotherhood:     brotherhoodRep = Mathf.Clamp(brotherhoodRep + amount, -100, 100); break;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref fightersRep, "fightersRep");
            Scribe_Values.Look(ref magesRep, "magesRep");
            Scribe_Values.Look(ref companionsRep, "companionsRep");
            Scribe_Values.Look(ref collegeRep, "collegeRep");
            Scribe_Values.Look(ref thievesRep, "thievesRep");
            Scribe_Values.Look(ref brotherhoodRep, "brotherhoodRep");
        }
    }
}
