namespace ESCP.Core;

public class ESCPSettings : ModSettings
{
    public Dictionary<string, bool> enlistStates = new Dictionary<string, bool>();
    public Dictionary<string, bool> options = new Dictionary<string, bool>();
    public int calendarEra = 3;
    public int calendarYear = 433;
    public int characterEra;
    public Action OnSaved;

    public int StartEra => characterEra == 0 ? calendarEra : characterEra;

    public int StartYear => characterEra == 0 ? calendarYear : 1;

    public bool AllowsEras(List<int> eras) => characterEra == 0 || eras.NullOrEmpty() || eras.Contains(characterEra);

    private List<string> enlistKeys;
    private List<bool> enlistValues;
    private List<string> optionKeys;
    private List<bool> optionValues;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref enlistStates, "enlistStates", LookMode.Value, LookMode.Value, ref enlistKeys, ref enlistValues);
        Scribe_Collections.Look(ref options, "options", LookMode.Value, LookMode.Value, ref optionKeys, ref optionValues);
        Scribe_Values.Look(ref calendarEra, "calendarEra", 3);
        Scribe_Values.Look(ref calendarYear, "calendarYear", 433);
        Scribe_Values.Look(ref characterEra, "characterEra");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            enlistStates ??= new Dictionary<string, bool>();
            options ??= new Dictionary<string, bool>();
        }
    }
}
