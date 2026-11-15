using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP.Factions
{
    [HarmonyPatch(typeof(Faction), nameof(Faction.LeaderTitle), MethodType.Getter)]
    public static class Faction_LeaderTitle_Patch
    {
        public static bool Prefix(Faction __instance, ref string __result)
        {
            var ext = __instance.def?.GetModExtension<FactionExtension_FlavorOverride>();
            if (ext?.preferFactionLeaderTitle != true)
                return true;

            if (__instance.leader.gender == Gender.Female && !__instance.def.leaderTitleFemale.NullOrEmpty())
                __result = __instance.def.leaderTitleFemale;
            else
                __result = __instance.def.leaderTitle;

            return false;
        }
    }

    [HarmonyPatch(typeof(FactionUIUtility), "DrawFactionRow")]
    public static class FactionUIUtility_DrawFactionRow_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo labelCapGetter = AccessTools.PropertyGetter(typeof(Def), nameof(Def.LabelCap));
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(labelCapGetter))
                    yield return CodeInstruction.Call(typeof(FactionUIUtility_DrawFactionRow_Patch), nameof(GetLabel));
                else
                    yield return instruction;
            }
        }

        private static TaggedString GetLabel(FactionDef def)
        {
            TaggedString label = FactionExtension_FlavorOverride.TryGetLabel(def) ?? def.LabelCap;
            return label;
        }
    }

    [HarmonyPatch(typeof(GoodwillSituationWorker_PermanentEnemy), "ArePermanentEnemies")]
    public static class GoodwillSituationWorker_PermanentEnemy_ArePermanentEnemies_Patch
    {
        public static void Postfix(Faction a, Faction b, ref bool __result)
        {
            if (__result)
                return;

            var aExt = a.def.GetModExtension<FactionExtension_PermanentlyHostileTo>();
            var bExt = b.def.GetModExtension<FactionExtension_PermanentlyHostileTo>();

            __result = aExt?.FactionIsHostileTo(b.def) ??
                       bExt?.FactionIsHostileTo(a.def) ??
                       __result;
        }
    }

    [HarmonyPatch(typeof(Faction), "CanChangeGoodwillFor")]
    public static class Faction_CanChangeGoodwillFor_Patch
    {
        public static void Postfix(Faction other, Faction __instance, ref bool __result)
        {
            if (!__result)
                return;

            var ext = __instance.def.GetModExtension<FactionExtension_PermanentlyHostileTo>();
            if (ext == null)
                return;

            __result = !ext.FactionIsHostileTo(other.def);
        }
    }

    [HarmonyPatch(typeof(FactionDef), "PermanentlyHostileTo")]
    public static class FactionDef_PermanentlyHostileTo_Patch
    {
        public static void Postfix(FactionDef otherFactionDef, FactionDef __instance, ref bool __result)
        {
            if (__result)
                return;

            var ext = __instance.GetModExtension<FactionExtension_PermanentlyHostileTo>();
            __result = ext?.FactionIsHostileTo(otherFactionDef) ?? false;
        }
    }

    [HarmonyPatch]
    public static class Faction_GetInitialGoodwill_Patch
    {
        public static MethodBase TargetMethod() =>
            typeof(Faction).GetDeclaredMethods().First(mi => mi.Name.Contains("GetInitialGoodwill"));

        public static bool Prefix(Faction a, Faction b, ref int __result)
        {
            var ext = a.def.GetModExtension<FactionExtension_PermanentlyHostileTo>();
            if (ext == null || !ext.hostileFactionDefs.Contains(b.def))
                return true;

            __result = -100;
            return false;
        }
    }
}
