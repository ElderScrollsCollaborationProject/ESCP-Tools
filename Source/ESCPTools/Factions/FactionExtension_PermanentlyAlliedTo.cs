using HarmonyLib;

namespace ESCP.Factions;

public class FactionExtension_PermanentlyAlliedTo : DefModExtension
{
    public List<FactionDef> alliedFactionDefs;

    public bool FactionIsAlliedTo(FactionDef other) => alliedFactionDefs.Contains(other);

    public static bool Allied(Faction a, Faction b)
    {
        return a.def.GetModExtension<FactionExtension_PermanentlyAlliedTo>()?.FactionIsAlliedTo(b.def) == true
            || b.def.GetModExtension<FactionExtension_PermanentlyAlliedTo>()?.FactionIsAlliedTo(a.def) == true;
    }
}

[HarmonyPatch(typeof(Faction), nameof(Faction.TryMakeInitialRelationsWith))]
public static class Faction_TryMakeInitialRelationsWith_PermanentAlly_Patch
{
    public static void Postfix(Faction __instance, Faction other)
    {
        if (__instance.IsPlayer || other.IsPlayer || !FactionExtension_PermanentlyAlliedTo.Allied(__instance, other))
            return;

        foreach (FactionRelation relation in new[] { __instance.RelationWith(other), other.RelationWith(__instance) })
        {
            relation.baseGoodwill = 100;
            relation.kind = FactionRelationKind.Ally;
        }
    }
}
