namespace ESCP.Core;

public class StorytellerLetter
{
    public string key;
    public string label;
    public string text;
    public int minDays;
    public int minColonists;
    public float minWealth;
    public FactionDef requiredFaction;

    public bool Ready()
    {
        return GenDate.DaysPassed >= minDays
            && (minColonists == 0 || PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.Count >= minColonists)
            && (minWealth == 0f || WealthUtility.PlayerWealth >= minWealth)
            && (requiredFaction == null || Find.FactionManager.FirstFactionOfDef(requiredFaction) != null);
    }
}

public class StorytellerExtension_Letters : DefModExtension
{
    public List<StorytellerLetter> letters = new List<StorytellerLetter>();
}

public class GameComponent_StorytellerLetters : GameComponent
{
    private const int CheckInterval = 2500;

    private HashSet<string> sent = new HashSet<string>();

    public GameComponent_StorytellerLetters(Game game)
    {
    }

    public override void GameComponentTick()
    {
        if (Find.TickManager.TicksGame % CheckInterval != 0)
            return;

        StorytellerExtension_Letters ext = Find.Storyteller.def.GetModExtension<StorytellerExtension_Letters>();
        if (ext == null || AuthorDead())
            return;

        foreach (StorytellerLetter letter in ext.letters)
        {
            if (sent.Contains(letter.key) || !letter.Ready())
                continue;

            sent.Add(letter.key);
            Find.LetterStack.ReceiveLetter(letter.label, letter.text, LetterDefOf.NeutralEvent);
            return;
        }
    }

    private static bool AuthorDead()
    {
        CharacterDef author = Find.Storyteller.def.GetModExtension<StorytellerExtension_Joiner>()?.characterDef;
        return author != null && UniqueCharactersTracker.Instance.CharacterPawnExists(author) && !UniqueCharactersTracker.Instance.CharacterPawnExistsAlive(author);
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref sent, "sent", LookMode.Value);
        if (Scribe.mode == LoadSaveMode.PostLoadInit && sent == null)
            sent = new HashSet<string>();
    }
}
