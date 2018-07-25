using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using System;

public class CalendarManager : MonoBehaviour
{
    public Text m_Current_Month;
    public Text m_Current_Year;

    public CalendarDay[] m_Days;

    Calendar calendar;
        
    int curYear = 1;
    int curMonth = 1;
    int curDay = 1;
    int monthsInYear = 12;
    int daysInMonth = 30;
    DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

    private void Awake()
    {
        Debug.Log("CalendarManager Awake()");

        calendar = new GregorianCalendar(GregorianCalendarTypes.Localized);

        curYear = calendar.GetYear(DateTime.Today);
        curMonth = calendar.GetMonth(DateTime.Today);
        curDay = calendar.GetDayOfMonth(DateTime.Today);

        monthsInYear = calendar.GetMonthsInYear(curYear);
        daysInMonth = calendar.GetDaysInYear(curYear);

        Debug.LogFormat("{0}, Year: {1}", calendar.GetType(), calendar.GetYear(DateTime.Today));
        Debug.LogFormat("   MonthsInYear: {0}", monthsInYear);
        Debug.LogFormat("   DaysInYear: {0}", daysInMonth);
        Debug.LogFormat("   Days in each month:");

        for (int j = 1; j <= calendar.GetMonthsInYear(curYear); j++)
            Debug.LogFormat(" {0,-5}", calendar.GetDaysInMonth(curYear, j));
        
        Debug.LogFormat("   IsLeapDay:   {0}", calendar.IsLeapDay(curYear, curMonth, curDay));
        Debug.LogFormat("   IsLeapMonth: {0}", calendar.IsLeapMonth(curYear, curMonth));
        Debug.LogFormat("   IsLeapYear:  {0}", calendar.IsLeapYear(curYear));
                                
        SetCalendarDays(curYear, curMonth, curDay);
        SetToday(curYear, curMonth, curDay);
    }

    private void SetCalendarDays(int _year, int _month, int _day)
    {
        if (m_Current_Year != null)
            m_Current_Year.text = _year.ToString();
        if (m_Current_Month != null)
            m_Current_Month.text = _month.ToString();

        DayOfWeek dayOfWeek = calendar.GetDayOfWeek(DateTime.Today);
        DateTime dt = new DateTime(curYear, curMonth, 1);
        firstDayOfWeek = calendar.GetDayOfWeek(dt);
        int firstDayIndex = (int)firstDayOfWeek;

        int dayCount = calendar.GetDaysInMonth(_year, _month);

        for (int i = 0; i < m_Days.Length; i++)
        {
            if (i >= firstDayIndex && i <= (dayCount + firstDayIndex - 1))
            {
                m_Days[i].dayImage.color = new Color32(255, 255, 255, 255);
                m_Days[i].dayText.text = (i - firstDayIndex + 1).ToString();
            }
            else
            {
                m_Days[i].dayImage.color = new Color32(0, 0, 0, 0);
                m_Days[i].dayText.text = "";
            }
        }
    }

    private void SetToday(int _year, int _month, int _day)
    {
        if (DateTime.Today.Year != _year)
            return;
        if (DateTime.Today.Month != _month)
            return;
        if (DateTime.Today.Day != _day)
            return;

        int firstDayIndex = (int)firstDayOfWeek;
        m_Days[firstDayIndex + _day - 1].dayImage.color = Color.yellow;
    }

    public void OnClick_Prev_Year()
    {
        curYear -= 1;
        if (curYear <= 1970)
            curYear = 1970;

        SetCalendarDays(curYear, curMonth, curDay);
        SetToday(curYear, curMonth, curDay);
    }

    public void OnClick_Next_Year()
    {
        curYear += 1;
        if (curYear >= 2030)
            curYear = 2030;

        SetCalendarDays(curYear, curMonth, curDay);
        SetToday(curYear, curMonth, curDay);
    }

    public void OnClick_Prev_Month()
    {
        curMonth -= 1;
        if (curMonth <= 1)
            curMonth = 1;

        SetCalendarDays(curYear, curMonth, curDay);
        SetToday(curYear, curMonth, curDay);
    }

    public void OnClick_Next_Month()
    {
        curMonth += 1;
        if (curMonth >= monthsInYear)
            curMonth = monthsInYear;

        SetCalendarDays(curYear, curMonth, curDay);
        SetToday(curYear, curMonth, curDay);
    }

    public void OnClick_Today()
    {
        curYear = calendar.GetYear(DateTime.Today);
        curMonth = calendar.GetMonth(DateTime.Today);
        curDay = calendar.GetDayOfMonth(DateTime.Today);
        SetCalendarDays(curYear, curMonth, curDay);
        SetToday(curYear, curMonth, curDay);
    }
}
