namespace ESCP.Core;

public class Alert_Notoriety : Alert
{
    public Alert_Notoriety()
    {
        defaultLabel = "ESCP_Alert_Notoriety_Label".Translate();
        defaultExplanation = "ESCP_Alert_Notoriety_Desc".Translate();
        defaultPriority = AlertPriority.Medium;
    }

    public override AlertReport GetReport() => NotorietyUtility.IsNotorious();
}
