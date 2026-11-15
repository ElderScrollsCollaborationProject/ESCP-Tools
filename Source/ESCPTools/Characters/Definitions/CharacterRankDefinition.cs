using ESCP.Ranks;

namespace ESCP.Core;

public class CharacterRankDefinition : CharacterBaseDefinition
{
    public RankTierDef tier;

    public override bool AppliesPreGeneration => false;
    public override bool AppliesPostGeneration => false;
}
