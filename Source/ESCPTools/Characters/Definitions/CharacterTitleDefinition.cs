using RimWorld;
using Verse;

namespace ESCP.Core
{
    public class CharacterTitleDefinition : CharacterBaseDefinition
    {
        public RoyalTitleDef title;

        public override bool AppliesPreGeneration => true;
        public override bool AppliesPostGeneration => title != null;

        public override void ApplyToRequest(ref PawnGenerationRequest request)
        {
            if (title == null) return;

            if (request.Faction != null && request.Faction.def.RoyalTitlesAllInSeniorityOrderForReading.Contains(title))
            {
                request.FixedTitle = title ?? request.FixedTitle;
                return;
            }

            Log.Warning($"[ESCP] CharacterTitleDefinition: title {title.LabelCap} ({title.defName}) set, but faction was invalid. Faction: {request.Faction?.Name ?? "null"}");
        }

        public override void ApplyToPawn(Pawn pawn)
        {
            pawn.royalty?.AllFactionPermits?.Clear();
        }
    }
}

