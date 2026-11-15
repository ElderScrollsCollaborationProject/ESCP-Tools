using HarmonyLib;
using UnityEngine;

namespace ESCP.Core;

[DefOf]
public static class CalendarDefOf
{
    public static ModOptionDef ESCP_Option_TamrielicCalendar;

    static CalendarDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(CalendarDefOf));
    }
}

[StaticConstructorOnStartup]
public static class TamrielicCalendar
{
    private const int LastEra = 4;
    private const int FirstTwelfthOfNewYear = 10;

    private static readonly int[] EraLengths = { 2920, 896, 433 };

    private static readonly string[] Months =
    {
        "ESCP_Month_FirstSeed", "ESCP_Month_RainsHand", "ESCP_Month_SecondSeed", "ESCP_Month_Midyear",
        "ESCP_Month_SunsHeight", "ESCP_Month_LastSeed", "ESCP_Month_Hearthfire", "ESCP_Month_Frostfall",
        "ESCP_Month_SunsDusk", "ESCP_Month_EveningStar", "ESCP_Month_MorningStar", "ESCP_Month_SunsDawn"
    };

    private static readonly string[] Weekdays =
    {
        "ESCP_Weekday_Sundas", "ESCP_Weekday_Morndas", "ESCP_Weekday_Tirdas", "ESCP_Weekday_Middas",
        "ESCP_Weekday_Turdas", "ESCP_Weekday_Fredas", "ESCP_Weekday_Loredas"
    };

    public static bool Active => CalendarDefOf.ESCP_Option_TamrielicCalendar.Enabled;

    static TamrielicCalendar()
    {
        ESCPToolsMod.Settings.OnSaved += DateReadout.Reset;
    }

    public static string Month(long absTicks, float longitude)
    {
        return MonthOfTwelfth((int)GenDate.Twelfth(absTicks, longitude));
    }

    public static string MonthOfTwelfth(int twelfth)
    {
        return Months[twelfth].Translate();
    }

    public static string Year(long absTicks, float longitude)
    {
        int era = EraTracker.CalendarEra;
        int year = GenDate.Year(absTicks, longitude);
        if ((int)GenDate.Twelfth(absTicks, longitude) >= FirstTwelfthOfNewYear)
            year++;

        while (era < LastEra && year > EraLengths[era - 1])
        {
            year -= EraLengths[era - 1];
            era++;
        }
        while (era > 1 && year < 1)
        {
            era--;
            year += EraLengths[era - 1];
        }
        return era + "E " + year;
    }

    public static int Day(long absTicks, float longitude)
    {
        return GenDate.DayOfTwelfth(absTicks, longitude) + 1;
    }

    public static string DayOfMonth(long absTicks, float longitude)
    {
        return "ESCP_Calendar_DayOfMonth".Translate(Find.ActiveLanguageWorker.OrdinalNumber(Day(absTicks, longitude)), Month(absTicks, longitude));
    }

    public static string FullDate(long absTicks, float longitude)
    {
        long day = (absTicks + GenDate.LocalTicksOffsetFromLongitude(longitude)) / GenDate.TicksPerDay;
        int weekday = (int)((day % Weekdays.Length + Weekdays.Length) % Weekdays.Length);
        return "ESCP_Calendar_FullDate".Translate(Weekdays[weekday].Translate(), DayOfMonth(absTicks, longitude), Year(absTicks, longitude));
    }
}

[HarmonyPatch(typeof(GenDate), nameof(GenDate.Year), typeof(long), typeof(float))]
public static class Patch_GenDate_Year_StartingYear
{
    public static bool Prefix(long absTicks, float longitude, ref int __result)
    {
        if (!TamrielicCalendar.Active)
            return true;
        long num = absTicks + GenDate.LocalTicksOffsetFromLongitude(longitude);
        __result = EraTracker.CalendarYear + Mathf.FloorToInt((float)num / GenDate.TicksPerYear);
        return false;
    }
}

[HarmonyPatch(typeof(GenDate), nameof(GenDate.DateFullStringAt))]
public static class GenDate_DateFullStringAt_Patch
{
    public static bool Prefix(long absTicks, Vector2 location, ref string __result)
    {
        if (!TamrielicCalendar.Active)
            return true;
        __result = TamrielicCalendar.FullDate(absTicks, location.x);
        return false;
    }
}

[HarmonyPatch(typeof(GenDate), nameof(GenDate.DateReadoutStringAt))]
public static class GenDate_DateReadoutStringAt_Patch
{
    public static bool Prefix(long absTicks, Vector2 location, ref string __result)
    {
        if (!TamrielicCalendar.Active)
            return true;
        __result = TamrielicCalendar.FullDate(absTicks, location.x);
        return false;
    }
}

[HarmonyPatch(typeof(GenDate), nameof(GenDate.DateMonthYearStringAt))]
public static class GenDate_DateMonthYearStringAt_Patch
{
    public static bool Prefix(long absTicks, Vector2 location, ref string __result)
    {
        if (!TamrielicCalendar.Active)
            return true;
        __result = "ESCP_Calendar_MonthYear".Translate(TamrielicCalendar.Month(absTicks, location.x), TamrielicCalendar.Year(absTicks, location.x));
        return false;
    }
}

[HarmonyPatch(typeof(GenDate), nameof(GenDate.DateShortStringAt))]
public static class GenDate_DateShortStringAt_Patch
{
    public static bool Prefix(long absTicks, Vector2 location, ref string __result)
    {
        if (!TamrielicCalendar.Active)
            return true;
        __result = "ESCP_Calendar_ShortDate".Translate(TamrielicCalendar.Day(absTicks, location.x), TamrielicCalendar.Month(absTicks, location.x), TamrielicCalendar.Year(absTicks, location.x));
        return false;
    }
}

[HarmonyPatch(typeof(GenDate), nameof(GenDate.QuadrumDateStringAt), typeof(long), typeof(float))]
public static class GenDate_QuadrumDateStringAt_Patch
{
    public static bool Prefix(long absTicks, float longitude, ref string __result)
    {
        if (!TamrielicCalendar.Active)
            return true;
        __result = TamrielicCalendar.DayOfMonth(absTicks, longitude);
        return false;
    }
}

[HarmonyPatch(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.AgeTooltipString), MethodType.Getter)]
public static class Pawn_AgeTracker_AgeTooltipString_Patch
{
    public static void Postfix(Pawn_AgeTracker __instance, ref string __result)
    {
        if (!TamrielicCalendar.Active)
            return;

        string vanilla = "FullDate".Translate(Find.ActiveLanguageWorker.OrdinalNumber(__instance.BirthDayOfSeasonZeroBased + 1), __instance.BirthQuadrum.Label(), __instance.BirthYear);
        __result = __result.Replace(vanilla, TamrielicCalendar.FullDate(__instance.BirthAbsTicks, 0f));
    }
}
