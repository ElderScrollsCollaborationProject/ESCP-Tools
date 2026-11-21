using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP_NecromanticThralls
{

    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.ThrallPatchesCategory)]
    [HarmonyPatch(typeof(Need))]
    class Need_IsFrozen_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("IsFrozen", MethodType.Getter)]
        public static bool IsFrozen_ThrallPatch(ref Need __instance, ref Pawn ___pawn, ref bool __result)
        {
            if (ESCP_NecromanticThralls_ModSettings.ThrallDisableNeeds && ThrallUtility.PawnIsThrall(___pawn))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.ThrallPatchesCategory)]
    [HarmonyPatch(typeof(Need))]
    class Need_ShowOnNeedList_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("ShowOnNeedList", MethodType.Getter)]
        public static bool ShowOnNeedList_ThrallPatch(ref Need __instance, ref Pawn ___pawn, ref bool __result)
        {
            if (ESCP_NecromanticThralls_ModSettings.ThrallDisableNeeds && ThrallUtility.PawnIsThrall(___pawn))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.ThrallPatchesCategory)]
    [HarmonyPatch(typeof(Need))]
    class Need_CurLevel_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("CurLevel", MethodType.Getter)]
        public static bool CurLevel_ThrallPatch(ref Need __instance, ref Pawn ___pawn, ref float __result)
        {
            if (ESCP_NecromanticThralls_ModSettings.ThrallDisableNeeds && ThrallUtility.PawnIsThrall(___pawn))
            {
                __result = __instance.MaxLevel;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.ThrallPatchesCategory)]
    [HarmonyPatch(typeof(Need))]
    class Need_CurInstantLevel_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("CurInstantLevel", MethodType.Getter)]
        public static bool CurInstantLevel_ThrallPatch(ref Need __instance, ref Pawn ___pawn, ref float __result)
        {
            if (ESCP_NecromanticThralls_ModSettings.ThrallDisableNeeds && ThrallUtility.PawnIsThrall(___pawn))
            {
                __result = __instance.MaxLevel;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.ThrallPatchesCategory)]
    [HarmonyPatch(typeof(Need_Chemical_Any))]
    class NeedChemicalAny_Disabled_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Disabled", MethodType.Getter)]
        public static void Disabled_ThrallPatch(ref Pawn ___pawn, ref bool __result)
        {
            if (!__result && ESCP_NecromanticThralls_ModSettings.ThrallDisableNeeds && ThrallUtility.PawnIsThrall(___pawn))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.ThrallPatchesCategory)]
    [HarmonyPatch(typeof(Need_Indoors))]
    class NeedIndoors_Disabled_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Disabled", MethodType.Getter)]
        public static void Disabled_ThrallPatch(ref Pawn ___pawn, ref bool __result)
        {
            if (!__result && ESCP_NecromanticThralls_ModSettings.ThrallDisableNeeds && ThrallUtility.PawnIsThrall(___pawn))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.ThrallPatchesCategory)]
    [HarmonyPatch(typeof(Need_Outdoors))]
    class NeedOutdoors_Disabled_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Disabled", MethodType.Getter)]
        public static void Disabled_ThrallPatch(ref Pawn ___pawn, ref bool __result)
        {
            if (!__result && ESCP_NecromanticThralls_ModSettings.ThrallDisableNeeds && ThrallUtility.PawnIsThrall(___pawn))
            {
                __result = true;
            }
        }
    }
}