using System;
using System.Collections.Generic;
using System.Text;

namespace DataRetrieve
{
    public class Utils
    {
        public static bool IsFirstDayOfYear(DateTime d, bool checkTime = false)
        {
            var ret = (d.Month == 1 && d.DayOfYear == 1);

            if (ret && checkTime)
            {
                ret = Utils.IsBeginOfDay(d);
            }

            return ret;
        }

        public static bool IsFirstDayOfMonth(DateTime d, bool checkTime = false)
        {
            var ret = (d.Day == 1);

            if (ret && checkTime)
            {
                ret = Utils.IsBeginOfDay(d);
            }

            return ret;
        }

        public static bool IsFirstDayOfWeek(DateTime d, bool checkTime = false)
        {
            var ret = (d.DayOfWeek == DayOfWeek.Monday);

            if (ret && checkTime)
            {
                ret = Utils.IsBeginOfDay(d);
            }

            return ret;
        }

        public static bool IsBeginOfDay(DateTime d)
        {
            return (d.Hour == 0 && d.Minute == 0 && d.Second == 0 && d.Millisecond == 0);
        }
    }
}
