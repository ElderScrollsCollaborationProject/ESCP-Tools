using RimWorld;
using Verse;

namespace ESCP.Imperials
{
    [DefOf]
    public static class ImperialGeneDefOf
    {
        [MayRequire("Rick.ESCP.Imperials")]
        public static GeneDef ESCP_Gene_Nibenean;
        [MayRequire("Rick.ESCP.Imperials")]
        public static GeneDef ESCP_Gene_Colovian;

        static ImperialGeneDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ImperialGeneDefOf));
        }
    }

    public class ThoughtWorker_NibeneanVsColovian : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            if (pawn.genes == null || other.genes == null)
                return ThoughtState.Inactive;

            if (!pawn.genes.HasActiveGene(ImperialGeneDefOf.ESCP_Gene_Nibenean))
                return ThoughtState.Inactive;

            if (!other.genes.HasActiveGene(ImperialGeneDefOf.ESCP_Gene_Colovian))
                return ThoughtState.Inactive;

            return ThoughtState.ActiveAtStage(0);
        }
    }

    public class ThoughtWorker_ColovianVsNibenean : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            if (pawn.genes == null || other.genes == null)
                return ThoughtState.Inactive;

            if (!pawn.genes.HasActiveGene(ImperialGeneDefOf.ESCP_Gene_Colovian))
                return ThoughtState.Inactive;

            if (!other.genes.HasActiveGene(ImperialGeneDefOf.ESCP_Gene_Nibenean))
                return ThoughtState.Inactive;

            return ThoughtState.ActiveAtStage(0);
        }
    }
}
