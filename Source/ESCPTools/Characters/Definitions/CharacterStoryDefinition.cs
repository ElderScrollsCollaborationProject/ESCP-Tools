using RimWorld;
using Verse;

namespace ESCP.Core
{
    public class CharacterStoryDefinition : CharacterBaseDefinition
    {
        public string firstName;
        public string lastName;
        public string nickname;
        public Gender? gender;
        public float? age;
        public float? chronologicalAge;
        public BackstoryDef adulthood;

        public override bool AppliesPreGeneration => gender != null || age != null || chronologicalAge != null;
        public override bool AppliesPostGeneration => !firstName.NullOrEmpty() || !lastName.NullOrEmpty() || !nickname.NullOrEmpty() || adulthood != null;

        public override void ApplyToRequest(ref PawnGenerationRequest request)
        {
            request.FixedGender = gender ?? request.FixedGender;
            request.FixedBiologicalAge = age ?? request.FixedBiologicalAge;
            request.FixedChronologicalAge = chronologicalAge ?? age ?? request.FixedChronologicalAge;
        }

        public override void ApplyToPawn(Pawn pawn)
        {
            if (pawn.Name is NameTriple oldName)
                pawn.Name = new NameTriple(firstName ?? oldName.First, nickname, lastName ?? oldName.Last);
            else if (!nickname.NullOrEmpty())
                pawn.Name = new NameSingle(nickname);

            if (adulthood != null)
            {
                foreach (SkillGain gain in pawn.story.Adulthood.skillGains)
                    pawn.skills.GetSkill(gain.skill).Level -= gain.amount;
                pawn.story.Adulthood = adulthood;
                foreach (SkillGain gain in adulthood.skillGains)
                    pawn.skills.GetSkill(gain.skill).Level += gain.amount;
                pawn.Notify_DisabledWorkTypesChanged();
                pawn.skills.Notify_SkillDisablesChanged();
            }
        }
    }
}
