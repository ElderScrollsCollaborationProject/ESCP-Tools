using System.Xml;
using HarmonyLib;
using Verse;

namespace ESCP.Core;

public class BackCompatibilityConverter_ESCP : BackCompatibilityConverter
{
    private const string OldThuumNamespace = "Thuum.";

    public static void Register()
    {
        AccessTools.StaticFieldRefAccess<List<BackCompatibilityConverter>>(typeof(BackCompatibility), "conversionChain").Add(new BackCompatibilityConverter_ESCP());
    }

    public override bool AppliesToVersion(int majorVer, int minorVer)
    {
        return true;
    }

    public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
    {
        return defType == typeof(SitePartDef) && defName == "ESCP_Site_TechCache" ? "ESCP_Site_RelicCache" : null;
    }

    public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
    {
        if (providedClassName.StartsWith(OldThuumNamespace))
            return GenTypes.GetTypeInAnyAssembly("ESCP.Core.Thuum." + providedClassName.Substring(OldThuumNamespace.Length));
        return null;
    }

    public override void PostExposeData(object obj)
    {
    }
}
