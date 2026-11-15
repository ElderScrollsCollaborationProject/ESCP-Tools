using RimWorld;
using Verse;

namespace ESCP.Currencies
{
    public class BullionBuyer : DefModExtension
    {
        public float sellPriceFactor = 1.25f;
    }

    public class StatPart_BullionBuyer : StatPart
    {
        public string tradeTag;

        public override void TransformValue(StatRequest req, ref float val)
        {
            float factor = FactorFor(req);
            if (factor != 1f)
                val *= factor;
        }

        public override string ExplanationPart(StatRequest req)
        {
            float factor = FactorFor(req);
            return factor != 1f ? "ESCP_BullionBuyerPrice".Translate(TradeSession.trader.TraderKind.label) + ": x" + factor.ToString("0.##") : null;
        }

        private float FactorFor(StatRequest req)
        {
            if (!TradeSession.Active || !(req.Def is ThingDef def) || def.tradeTags == null || !def.tradeTags.Contains(tradeTag))
                return 1f;
            return TradeSession.trader.TraderKind?.GetModExtension<BullionBuyer>()?.sellPriceFactor ?? 1f;
        }
    }
}
