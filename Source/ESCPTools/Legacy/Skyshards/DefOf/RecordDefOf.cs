using RimWorld;

namespace ESCP_Skyshards
{
    [DefOf]
    public static class RecordDefOf
    {
        static RecordDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RecordDefOf));
        }

        [MayRequire("Rick.ESCP.Skyshards")]

        public static RecordDef ESCP_SkyshardsUsed;
    }
}
