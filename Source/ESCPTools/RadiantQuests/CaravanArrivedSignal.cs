using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ESCP.Core.RadiantQuests;

[HarmonyPatch(typeof(CaravanArrivalAction_VisitSettlement), nameof(CaravanArrivalAction_VisitSettlement.Arrived))]
public static class CaravanArrivalAction_VisitSettlement_Arrived_Patch
{
    public static void Prefix(Settlement ___settlement, Caravan caravan) => CaravanArrivedSignal.Send(___settlement, caravan);
}

[HarmonyPatch(typeof(CaravanArrivalAction_Trade), nameof(CaravanArrivalAction_Trade.Arrived))]
public static class CaravanArrivalAction_Trade_Arrived_Patch
{
    public static void Prefix(Settlement ___settlement, Caravan caravan) => CaravanArrivedSignal.Send(___settlement, caravan);
}

public static class CaravanArrivedSignal
{
    public static void Send(Settlement settlement, Caravan caravan)
    {
        if (!settlement.questTags.NullOrEmpty())
            QuestUtility.SendQuestTargetSignals(settlement.questTags, "CaravanArrived", settlement.Named("SUBJECT"), caravan.Named("CARAVAN"));
    }
}
