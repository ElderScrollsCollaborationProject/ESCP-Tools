using HarmonyLib;
using UnityEngine;
using Verse.AI;

namespace ESCP.Core;

public class RaceExtension_Quadruped : DefModExtension
{
    public List<BodyTypeDef> bodyTypes = new List<BodyTypeDef>();
    public List<BodyTypeDef> femaleBodyTypes = new List<BodyTypeDef>();
    public BodyTypeDef childBodyType;
    public Vector2 headOffsetSouth;
    public Vector2 headOffsetNorth;
    public Vector2 headOffsetEast;
}

[HarmonyPatch(typeof(ApparelUtility), nameof(ApparelUtility.HasPartsToWear))]
public static class ApparelUtility_HasPartsToWear_Patch
{
    public static void Postfix(ref bool __result, Pawn p, ThingDef apparel)
    {
        if (!__result)
            return;

        XenotypeExtension ext = apparel.GetModExtension<XenotypeExtension>();
        XenotypeDef xenotype = p.genes?.Xenotype;
        if (ext?.whitelistedXenotypes != null)
            __result = ext.whitelistedXenotypes.Contains(xenotype);
        else if (p.def.HasModExtension<RaceExtension_Quadruped>())
            __result = false;
        else if (ext?.blacklistedXenotypes != null)
            __result = !ext.blacklistedXenotypes.Contains(xenotype);
    }
}

[HarmonyPatch(typeof(ResearchProjectDef), nameof(ResearchProjectDef.CanStartNow), MethodType.Getter)]
public static class ResearchProjectDef_CanStartNow_Patch
{
    public static void Postfix(ref bool __result, ResearchProjectDef __instance)
    {
        if (!__result)
            return;

        List<XenotypeDef> allowed = __instance.GetModExtension<XenotypeExtension>()?.whitelistedXenotypes;
        if (allowed == null)
            return;

        List<Pawn> colonists = PawnsFinder.AllMaps_FreeColonists;
        for (int i = 0; i < colonists.Count; i++)
        {
            if (allowed.Contains(colonists[i].genes?.Xenotype))
                return;
        }
        __result = false;
    }
}

[HarmonyPatch(typeof(Bill), nameof(Bill.PawnAllowedToStartAnew))]
public static class Bill_PawnAllowedToStartAnew_Product_Patch
{
    public static void Postfix(ref bool __result, Bill __instance, Pawn p)
    {
        if (!__result)
            return;

        List<XenotypeDef> allowed = __instance.recipe.ProducedThingDef?.GetModExtension<XenotypeExtension>()?.craftableByXenotypes;
        if (allowed == null || allowed.Contains(p.genes?.Xenotype))
            return;

        JobFailReason.Is("ESCP_Recipe_XenotypeRequired".Translate(allowed[0].label));
        __result = false;
    }
}

[HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GetBodyTypeFor))]
public static class PawnGenerator_GetBodyTypeFor_Patch
{
    public static void Postfix(ref BodyTypeDef __result, Pawn pawn)
    {
        RaceExtension_Quadruped ext = pawn.def.GetModExtension<RaceExtension_Quadruped>();
        if (ext != null)
            __result = pawn.DevelopmentalStage.Juvenile() ? ext.childBodyType : (pawn.gender == Gender.Female && ext.femaleBodyTypes.Any() ? ext.femaleBodyTypes : ext.bodyTypes).RandomElement();
    }
}

[HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.BaseHeadOffsetAt))]
public static class PawnRenderer_BaseHeadOffsetAt_Patch
{
    public static void Postfix(ref Vector3 __result, Pawn ___pawn, Rot4 rotation)
    {
        Pawn pawn = ___pawn;
        if (pawn.def == ThingDefOf.Human || pawn.DevelopmentalStage.Juvenile())
            return;

        RaceExtension_Quadruped ext = pawn.def.GetModExtension<RaceExtension_Quadruped>();
        if (ext == null)
            return;

        Vector2 offset = rotation == Rot4.South ? ext.headOffsetSouth : rotation == Rot4.North ? ext.headOffsetNorth : ext.headOffsetEast;
        __result += new Vector3(rotation == Rot4.West ? -offset.x : offset.x, 0f, offset.y);
    }
}

public class PawnRenderNodeProperties_FurPattern : PawnRenderNodeProperties
{
    public bool onHead;
    public int variants;
    public float darken = 0.55f;
}

public class PawnRenderNode_FurPattern : PawnRenderNode
{
    public PawnRenderNode_FurPattern(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
        : base(pawn, props, tree)
    {
    }

    public override GraphicMeshSet MeshSetFor(Pawn pawn)
    {
        return ((PawnRenderNodeProperties_FurPattern)props).onHead ? HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn) : HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(pawn);
    }

    public override Graphic GraphicFor(Pawn pawn)
    {
        var pattern = (PawnRenderNodeProperties_FurPattern)props;
        int variant = pawn.thingIDNumber % (pattern.variants + 1);
        if (variant == 0 || pawn.story?.bodyType == null || pawn.story.headType == null)
            return null;

        string shape = pattern.onHead ? System.IO.Path.GetFileName(pawn.story.headType.graphicPath) : pawn.story.bodyType.defName;
        string path = props.texPath + shape + variant;
        if (ContentFinder<Texture2D>.Get(path + "_south", false) == null)
            return null;

        return GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutComplex, Vector2.one, pawn.story.HairColor * pattern.darken, pawn.story.HairColor);
    }
}
