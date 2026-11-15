using RimWorld.Planet;

namespace ESCP.Core;

public class EraTracker : WorldComponent
{
    public int era;
    public int calendarEra;
    public int calendarYear;

    private static EraTracker instance;

    public EraTracker(World world) : base(world)
    {
        instance = this;
        ESCPSettings settings = ESCPToolsMod.Settings;
        era = settings.characterEra;
        calendarEra = settings.StartEra;
        calendarYear = settings.StartYear;
    }

    private static EraTracker Active => instance != null && instance.world == Find.World ? instance : null;

    public static int Era => Active?.era ?? ESCPToolsMod.Settings.characterEra;

    public static int CalendarEra => Active?.calendarEra ?? ESCPToolsMod.Settings.StartEra;

    public static int CalendarYear => Active?.calendarYear ?? ESCPToolsMod.Settings.StartYear;

    public static bool Allows(List<int> eras)
    {
        int current = Era;
        return current == 0 || eras.NullOrEmpty() || eras.Contains(current);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        ESCPSettings settings = ESCPToolsMod.Settings;
        Scribe_Values.Look(ref era, "era");
        Scribe_Values.Look(ref calendarEra, "calendarEra", settings.calendarEra);
        Scribe_Values.Look(ref calendarYear, "calendarYear", settings.calendarYear);
    }
}
