namespace ESCP.Core;

public class ModOptionDef : Def
{
    public bool defaultValue;
    public bool worldGenOnly;

    public bool Enabled
    {
        get => ESCPToolsMod.Settings.options.TryGetValue(defName, out bool value) ? value : defaultValue;
        set => ESCPToolsMod.Settings.options[defName] = value;
    }
}
