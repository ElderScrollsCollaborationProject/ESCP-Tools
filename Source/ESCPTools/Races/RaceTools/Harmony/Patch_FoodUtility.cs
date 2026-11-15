using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP_RaceTools
{
	[HarmonyPatchCategory(ESCP.Core.ESCPToolsMod.LatePatchesCategory)]
	[HarmonyPatch(typeof(FoodUtility))]
	[HarmonyPatch("GetFoodPoisonChanceFactor")]
	public static class FoodUtility_GetFoodPoisonChanceFactor_Patch
	{
		public static void Postfix(Pawn ingester, ref float __result)
		{
			if (__result > 0f)
			{
				RaceProperties props = RaceProperties.For(ingester);

				if (props != null && props.foodPoisoningResistant)
				{
					__result = 0f;
				}
			}
		}
	}
}
