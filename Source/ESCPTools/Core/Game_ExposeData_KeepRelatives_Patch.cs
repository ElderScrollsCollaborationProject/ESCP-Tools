using HarmonyLib;
using RimWorld.Planet;

namespace ESCP.Core;

[HarmonyPatch(typeof(Game), nameof(Game.ExposeData))]
[HarmonyPatchCategory(ESCPToolsMod.LatePatchesCategory)]
public static class Game_ExposeData_KeepRelatives_Patch
{
    public static void Prefix()
    {
        if (Scribe.mode != LoadSaveMode.Saving)
            return;

        var handled = new HashSet<Pawn>();
        var relatives = new List<Pawn>();
        do
        {
            relatives.Clear();
            foreach (Pawn pawn in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
            {
                if (pawn.relations == null)
                    continue;
                foreach (DirectPawnRelation relation in pawn.relations.DirectRelations)
                {
                    Pawn other = relation.otherPawn;
                    if (!other.Destroyed && !other.Discarded && !WillBeSaved(other) && handled.Add(other))
                        relatives.Add(other);
                }
            }
            foreach (Pawn relative in relatives)
                Find.WorldPawns.PassToWorld(relative, PawnDiscardDecideMode.KeepForever);
        }
        while (relatives.Count > 0);

        foreach (Pawn pawn in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
        {
            if (pawn.relations == null)
                continue;
            foreach (DirectPawnRelation relation in pawn.relations.DirectRelations.ToList())
            {
                if (!WillBeSaved(relation.otherPawn))
                    pawn.relations.RemoveDirectRelation(relation);
            }
        }

        foreach (RelationshipRecord record in Find.RelationshipRecords.Records.Values)
        {
            foreach (Pawn reference in record.References.ToList())
            {
                if (!WillBeSaved(reference))
                    record.RemoveReference(reference);
            }
        }
    }

    private static bool WillBeSaved(Pawn pawn)
    {
        return !pawn.Discarded && (pawn.Spawned || pawn.IsWorldPawn() || pawn.ParentHolder != null);
    }
}
