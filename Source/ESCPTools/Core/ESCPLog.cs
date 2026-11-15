namespace ESCP.Core;

public static class ESCPLog
{
    private const string Prefix = "<color=#E0C068FF>[ESCP Tools] </color>";

    public static void Error(object msg) => Log.Error(Prefix + msg);

    public static void Warning(object msg) => Log.Warning(Prefix + msg);

    public static void Message(object msg) => Log.Message(Prefix + msg);
}
