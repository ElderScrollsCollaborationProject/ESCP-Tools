using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ESCP.Factions
{
    [StaticConstructorOnStartup]
    public static class FactionXenotypes
    {
        static readonly AccessTools.FieldRef<XenotypeSet, List<XenotypeChance>> chancesRef =
            AccessTools.FieldRefAccess<XenotypeSet, List<XenotypeChance>>("xenotypeChances");

        static readonly (string defName, float weight)[] tamrielMix =
        {
            ("ESCP_Xenotype_Imperial", 4f),
            ("ESCP_Xenotype_Nord", 4f),
            ("ESCP_Xenotype_Breton", 2.5f),
            ("ESCP_Xenotype_Redguard", 2.5f),
            ("ESCP_Xenotype_Dunmer", 2f),
            ("ESCP_Xenotype_Bosmer", 1.5f),
            ("ESCP_Xenotype_Orsimer", 1.5f),
            ("ESCP_Xenotype_Altmer", 1f),
            ("ESCP_Xenotype_Khajiit", 1f)
        };

        static FactionXenotypes()
        {
            if (!ModsConfig.BiotechActive)
                return;

            var mix = new List<(XenotypeDef, float)>();
            float mixTotal = 0f;
            foreach (var (defName, weight) in tamrielMix)
            {
                XenotypeDef xenotype = DefDatabase<XenotypeDef>.GetNamedSilentFail(defName);
                if (xenotype == null)
                    continue;
                mix.Add((xenotype, weight));
                mixTotal += weight;
            }

            var weights = new Dictionary<XenotypeDef, float>();
            foreach (FactionDef faction in DefDatabase<FactionDef>.AllDefsListForReading)
            {
                if (faction.isPlayer || !faction.humanlikeFaction || faction.pawnGroupMakers.NullOrEmpty() || !faction.defName.StartsWith("ESCP_"))
                    continue;
                if (faction.xenotypeSet != null && faction.xenotypeSet.BaselinerChance < 1f)
                    continue;

                weights.Clear();
                float total = 0f;
                foreach (PawnGroupMaker maker in faction.pawnGroupMakers)
                {
                    foreach (List<PawnGenOption> options in new[] { maker.options, maker.traders, maker.carriers, maker.guards })
                    {
                        if (options == null)
                            continue;
                        foreach (PawnGenOption option in options)
                        {
                            if (!option.kind.RaceProps.Humanlike)
                                continue;
                            total += option.selectionWeight;
                            XenotypeSet set = option.kind.xenotypeSet;
                            if (set == null)
                            {
                                foreach (var (xenotype, weight) in mix)
                                    Add(weights, xenotype, weight / mixTotal * option.selectionWeight);
                                continue;
                            }
                            option.kind.useFactionXenotypes = false;
                            for (int i = 0; i < set.Count; i++)
                            {
                                if (set[i].xenotype != XenotypeDefOf.Baseliner)
                                    Add(weights, set[i].xenotype, set[i].chance * option.selectionWeight);
                            }
                        }
                    }
                }

                if (total <= 0f || weights.Count == 0)
                    continue;

                faction.xenotypeSet ??= new XenotypeSet();
                List<XenotypeChance> chances = chancesRef(faction.xenotypeSet);
                chances.Clear();
                foreach (var (xenotype, weight) in weights)
                    chances.Add(new XenotypeChance(xenotype, weight / total));
            }
        }

        static void Add(Dictionary<XenotypeDef, float> weights, XenotypeDef xenotype, float weight)
        {
            weights.TryGetValue(xenotype, out float w);
            weights[xenotype] = w + weight;
        }
    }
}
