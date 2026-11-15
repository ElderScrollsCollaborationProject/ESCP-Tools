using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ESCP.Core
{
    public class UniqueThingExtension : DefModExtension
    {
    }

    public class CharacterUniqueItemDefinition : CharacterBaseDefinition
    {
        public ThingDef thingDef;
        public ThingDef stuffDef;
        public ThingDef usedAs;
        public QualityCategory quality = QualityCategory.Normal;
        public bool isWeapon;
        public bool isApparel;
        public bool isInventory;
        public List<ThingDef> possibleThings;

        public override bool AppliesPreGeneration => false;
        public override bool AppliesPostGeneration => true;

        public override void ApplyToPawn(Pawn pawn)
        {
            ThingDef chosenDef = thingDef;
            if (chosenDef == null && !possibleThings.NullOrEmpty())
                chosenDef = possibleThings.RandomElement();

            if (chosenDef == null)
            {
                Log.Warning("[ESCP] CharacterUniqueItemDefinition: no thingDef or possibleThings specified.");
                return;
            }

            UniqueCharactersTracker tracker = UniqueCharactersTracker.Instance;
            bool isUnique = chosenDef.HasModExtension<UniqueThingExtension>();
            if (isUnique && tracker != null)
            {
                if (tracker.IsUniqueThingCreated(chosenDef))
                    return;
                tracker.Notify_UniqueThingSpawned(chosenDef);
            }

            Thing item = ThingMaker.MakeThing(chosenDef, stuffDef);
            item.TryGetComp<CompQuality>()?.SetQuality(quality, ArtGenerationContext.Outsider);

            if (isApparel && item is Apparel apparel)
            {
                pawn.apparel?.Wear(apparel, dropReplacedApparel: false);
            }
            else if (isWeapon)
            {
                pawn.equipment?.AddEquipment(item as ThingWithComps);
            }
            else if (isInventory)
            {
                pawn.inventory?.TryAddItemNotForSale(item);
            }
        }
    }
}
