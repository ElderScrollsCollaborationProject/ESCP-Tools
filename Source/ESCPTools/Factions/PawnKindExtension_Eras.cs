using HarmonyLib;
using ESCP.Core;

namespace ESCP.Factions;

public class PawnKindExtension_Eras : DefModExtension
{
    public List<int> eras;
}

[HarmonyPatch(typeof(PawnGroupMakerUtility), nameof(PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints))]
public static class PawnGroupMakerUtility_ChoosePawnGenOptionsByPoints_Eras_Patch
{
    public static void Prefix(ref List<PawnGenOption> options)
    {
        if (options == null)
            return;
        for (int i = 0; i < options.Count; i++)
        {
            List<int> eras = options[i].kind.GetModExtension<PawnKindExtension_Eras>()?.eras;
            if (eras != null && !EraTracker.Allows(eras))
            {
                options = options.Where(o => EraTracker.Allows(o.kind.GetModExtension<PawnKindExtension_Eras>()?.eras)).ToList();
                return;
            }
        }
    }
}
