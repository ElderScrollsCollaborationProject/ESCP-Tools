using HarmonyLib;
using UnityEngine;

namespace ESCP.Core;

[StaticConstructorOnStartup]
public static class DraugrUtility
{
    public static readonly MutantDef Mutant = DefDatabase<MutantDef>.GetNamedSilentFail("ESCP_Mutant_Draugr");

    public static bool IsDraugr(this Pawn pawn)
    {
        return Mutant != null && pawn.mutant != null && pawn.mutant.Def == Mutant;
    }
}

public class PawnRenderNodeWorker_OverlayDraugr : PawnRenderNodeWorker_Overlay
{
    protected override PawnOverlayDrawer OverlayDrawer(Pawn pawn)
    {
        return pawn.Drawer.renderer.ShamblerScarDrawer;
    }

    public override bool ShouldListOnGraph(PawnRenderNode node, PawnDrawParms parms)
    {
        return parms.pawn.IsDraugr();
    }

    public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
    {
        return base.CanDrawNow(node, parms) && parms.pawn.IsDraugr() && parms.pawn.mutant.HasTurned;
    }
}

[HarmonyPatch(typeof(MutantUtility), nameof(MutantUtility.SetFreshPawnAsMutant))]
public static class MutantUtility_SetFreshPawnAsMutant_Draugr_Patch
{
    public static bool Prefix(Pawn pawn, MutantDef mutant)
    {
        if (mutant != DraugrUtility.Mutant)
            return true;
        if (pawn.equipment?.Primary != null)
            pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
        MutantUtility.SetPawnAsMutantInstantly(pawn, mutant, RotStage.Rotting);
        if (pawn.equipment != null)
            PawnWeaponGenerator.TryGenerateWeaponFor(pawn, new PawnGenerationRequest(pawn.kindDef, pawn.Faction));
        return false;
    }
}

[HarmonyPatch(typeof(Pawn_StoryTracker), nameof(Pawn_StoryTracker.HairColor), MethodType.Getter)]
public static class Pawn_StoryTracker_HairColor_Draugr_Patch
{
    static readonly Color DraugrHair = new Color(0.72f, 0.76f, 0.82f);

    public static void Postfix(Pawn ___pawn, ref Color __result)
    {
        if (___pawn.IsDraugr())
            __result = DraugrHair;
    }
}

[HarmonyPatch(typeof(PawnRenderNode), nameof(PawnRenderNode.ColorFor))]
public static class PawnRenderNode_ColorFor_Draugr_Patch
{
    static readonly Color DraugrSkin = new Color(0.42f, 0.55f, 0.7f);

    public static void Postfix(PawnRenderNode __instance, Pawn pawn, ref Color __result)
    {
        if (__instance.Props.colorType == PawnRenderNodeProperties.AttachmentColorType.Skin && pawn.IsDraugr())
            __result = Color.Lerp(__result, DraugrSkin, 0.6f);
    }
}

[HarmonyPatch(typeof(Pawn), nameof(Pawn.SetFaction))]
public static class Pawn_SetFaction_Draugr_Patch
{
    public static bool Prefix(Pawn __instance, Faction newFaction)
    {
        return newFaction == null || !newFaction.IsPlayer || !__instance.IsDraugr();
    }
}
