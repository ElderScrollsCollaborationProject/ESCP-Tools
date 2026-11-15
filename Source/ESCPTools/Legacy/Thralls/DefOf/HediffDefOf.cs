using Verse;
using RimWorld;

namespace ESCP_NecromanticThralls
{
    [DefOf]
    public static class HediffDefOf
	{
		static HediffDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
		}

		[MayRequire("Rick.ESCP.NecromanticThralls")]

		public static HediffDef ESCP_NecromanticThralls_Enthralled;
		[MayRequire("Rick.ESCP.NecromanticThralls")]
		public static HediffDef ESCP_NecromanticThralls_ThrallStorage;
		[MayRequire("Rick.ESCP.NecromanticThralls")]
		public static HediffDef ESCP_NecromanticThralls_ThrallLifespan;
	}
}
