using System.Collections.Generic;
using Verse;
using RimWorld;
using System;

namespace ESCP_RaceTools
{
    public class RaceProperties : DefModExtension
    {

        public bool completeHypothermiaResistance = false;
        public bool hypothermiaResistant = false;
        public bool frostbiteResistant = false;
        public HediffDef hypothermiaDef;

        public bool completeHeatstrokeResistance = false;
        public bool heatstrokeResistant = false;
        public bool heatburnResistant = false;
        public HediffDef heatstrokeDef;

        public bool foodPoisoningResistant = false;

        public ThingDef leatherDef;

        public bool modifiedExpectations = false;
        public int expectationOffset = -1;

        public HistoryEventDef eventOnDeath;

        public bool immuneToAllDisease = false;
        public List<string> immuneDiseases;

        public List<HediffDef> hediffsToAdd;
        public List<AbilityDef> abilitiesToAdd;
        public List<AbilityDef> oneOfRandomAbility;
        public float oneOfRandomAbilityChance = 1f;

        [Obsolete]
        public List<GeneDef> genesToAdd;

        public GeneDef majorRacialGeneToInherit;
        public GeneDef minorRacialGeneToInherit;

        public bool sloadThrallImmune = false;
        public bool thrassianPlagueImmune = false;

        public List<BodyPartGroupDef> restrictedApparelPartGroups;
        public string restrictedApparelOverrideTag;

        public bool removeFacialAnimationsComps = false;

        public static RaceProperties Get(Def def)
        {
            return def.GetModExtension<RaceProperties>();
        }

        public static RaceProperties For(Pawn pawn)
        {
            return pawn.genes?.Xenotype?.GetModExtension<RaceProperties>() ?? pawn.def.GetModExtension<RaceProperties>();
        }
    }
}
