using Verse;
using RimWorld;

namespace ESCP_Trolls
{
	[DefOf]
	public static class PawnKindDefOf
	{

		static PawnKindDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf));
		}

		[MayRequire("Rick.ESCP.Trolls")]

		public static PawnKindDef ESCP_TrollAnima;

	}
}
