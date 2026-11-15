using ESCP.Core;
using HarmonyLib;

namespace ESCP.Factions;

public class EraRelationsDef : Def
{
    public List<int> eras = new List<int>();
    public List<FactionDef> allies = new List<FactionDef>();
    public List<FactionDef> enemies = new List<FactionDef>();

    public bool Active => EraTracker.Era != 0 && eras.Contains(EraTracker.Era);

    public FactionRelationKind? RelationBetween(FactionDef a, FactionDef b)
    {
        if (allies.Contains(a) && enemies.Contains(b) || allies.Contains(b) && enemies.Contains(a))
            return FactionRelationKind.Hostile;
        if (allies.Contains(a) && allies.Contains(b))
            return FactionRelationKind.Ally;
        return null;
    }
}

[HarmonyPatch(typeof(Faction), nameof(Faction.TryMakeInitialRelationsWith))]
[HarmonyPriority(Priority.Low)]
public static class Faction_TryMakeInitialRelationsWith_Era_Patch
{
    public static void Postfix(Faction __instance, Faction other)
    {
        if (__instance.IsPlayer || other.IsPlayer)
            return;

        FactionRelationKind? kind = null;
        foreach (EraRelationsDef def in DefDatabase<EraRelationsDef>.AllDefsListForReading)
        {
            if (!def.Active)
                continue;
            FactionRelationKind? found = def.RelationBetween(__instance.def, other.def);
            if (found == FactionRelationKind.Hostile)
            {
                kind = found;
                break;
            }
            kind ??= found;
        }
        if (kind == null)
            return;

        foreach (FactionRelation relation in new[] { __instance.RelationWith(other), other.RelationWith(__instance) })
        {
            relation.baseGoodwill = kind == FactionRelationKind.Ally ? 100 : -100;
            relation.kind = kind.Value;
        }
    }
}
