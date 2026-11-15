using RimWorld;

namespace ESCP_NecromanticThralls
{
	[DefOf]
	public static class ThoughtDefOf
	{

		static ThoughtDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDefOf));
		}

		[MayRequire("Rick.ESCP.NecromanticThralls")]

		public static ThoughtDef ESCP_NecromanticThralls_ThrallThought;
	}
}
