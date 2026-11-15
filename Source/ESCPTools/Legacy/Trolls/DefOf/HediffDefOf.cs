using Verse;
using RimWorld;

namespace ESCP_Trolls
{
	[DefOf]
	public static class HediffDefOf
	{

		static HediffDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
		}

		[MayRequire("Rick.ESCP.Trolls")]

		public static HediffDef ESCP_AfflictedTroll;
		[MayRequire("Rick.ESCP.Trolls")]
		public static HediffDef ESCP_AfflictedTrollBattleScar;
	}
}
