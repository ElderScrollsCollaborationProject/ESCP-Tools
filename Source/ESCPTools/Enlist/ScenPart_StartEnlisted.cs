using System.Linq;
using ESCP.Core;
using RimWorld;
using Verse;

namespace ESCP.Enlist;

public class ScenPart_StartEnlisted : ScenPart
{
    public FactionDef factionDef;
    public FactionEnlistOptionsDef enlistOptionsDef;

    public override void PostGameStart()
    {
        base.PostGameStart();

        if (factionDef == null)
        {
            ESCPLog.Warning("ScenPart_StartEnlisted has no factionDef set.");
            return;
        }

        Faction faction = Find.FactionManager.AllFactionsListForReading
            .FirstOrDefault(f => f.def == factionDef && !f.IsPlayer);
        if (faction == null)
        {
            ESCPLog.Warning($"ScenPart_StartEnlisted found no faction for {factionDef.defName} in this world.");
            return;
        }

        FactionEnlistOptionsDef optionsDef = enlistOptionsDef ?? faction.GetEnlistOptions().FirstOrDefault();
        if (optionsDef == null)
        {
            ESCPLog.Warning($"ScenPart_StartEnlisted for {factionDef.defName} has no FactionEnlistOptionsDef to enlist with.");
            return;
        }

        optionsDef.Worker.EnlistTo(faction);
    }

    public override string Summary(Scenario scen)
    {
        return "ESCP_ScenPart_StartEnlisted".Translate(factionDef?.label ?? "?");
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref factionDef, "factionDef");
        Scribe_Defs.Look(ref enlistOptionsDef, "enlistOptionsDef");
    }
}
