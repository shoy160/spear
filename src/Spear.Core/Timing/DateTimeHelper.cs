using System;

namespace Spear.Core.Timing
{
    /// <summary> 时间戳类型 </summary>
    public enum TimestampType : byte
    {
        /// <summary> 秒 </summary>
        Second,
        /// <summary> 毫秒 </summary>
        MilliSecond
    }

    /// <summary> 时间辅助类 </summary>
    public static class DateTimeHelper
    {
        /// <summary> 起始时间 </summary>
        public static DateTime ZoneTime = new DateTime(1970, 1, 1);

        /// <summary> 转换成时间戳 </summary>
        /// <param name="dateTime">需要转换的时间</param>
        /// <param name="kind">转换的时间类型（默认为UTC）</param>
        /// <param name="type">时间戳类型</param>
        /// <returns>时间戳</returns>
        public static long ToTimestamp(this DateTime dateTime, DateTimeKind kind = DateTimeKind.Utc, TimestampType type = TimestampType.Second)
        {
            switch (kind)
            {
                case DateTimeKind.Utc:
                    dateTime = dateTime.ToUniversalTime();
                    break;
                case DateTimeKind.Local:
                    dateTime = dateTime.ToLocalTime();
                    break;
            }

            var timespan = dateTime.ToUniversalTime() - ZoneTime;
            if (type == TimestampType.Second)
                return (long)timespan.TotalSeconds;
            return (long)timespan.TotalMilliseconds;
        }

        /// <summary> 转换成时间戳(毫秒) </summary>
        /// <param name="dateTime">需要转换的时间</param>
        /// <param name="kind">转换的时间类型（默认为UTC）</param>
        /// <returns>时间戳</returns>
        public static long ToMillisecondsTimestamp(this DateTime dateTime, DateTimeKind kind = DateTimeKind.Utc)
        {
            return dateTime.ToTimestamp(kind, TimestampType.MilliSecond);
        }

        /// <summary>
        /// 时间戳转换成日期
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns></returns>
        public static DateTime FromTimestamp(this long timestamp)
        {
            return ZoneTime.Add(new TimeSpan(timestamp * TimeSpan.TicksPerSecond)).ToLocalTime();
        }

        /// <summary>
        /// 时间戳转换成日期
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <returns></returns>
        public static DateTime FromMillisecondTimestamp(this long timestamp)
        {
            return ZoneTime.Add(new TimeSpan(timestamp * TimeSpan.TicksPerMillisecond)).ToLocalTime();
        }

        #region 日
        /// <summary>
        /// 将日期转换为本日的开始时间
        /// </summary>
        /// <param name="value">2001-01-01</param>
        /// <returns>2001-01-01 00:00:00</returns>
        public static DateTime ToDayStart(string value)
        {
            //转换成日期类型
            var date = Convert.ToDateTime(value);
            return date.ToDayStart();
        }

        /// <summary>
        /// 将日期转换为本日的开始时间
        /// </summary>
        /// <param name="value">任意时间</param>
        /// <returns>2001-01-01 00:00:00</returns>
        public static DateTime ToDayStart(this DateTime value)
        {
            return value.Date;
        }

        /// <summary>
        /// 将日期转换为本日的开始时间
        /// </summary>
        /// <param name="value">2001-01-01</param>
        /// <returns>2001-01-01 23:59:59</returns>
        public static DateTime ToDayEnd(string value)
        {
            //转换成日期类型
            var date = Convert.ToDateTime(value);
            return date.ToDayEnd();
        }

        /// <summary>
        /// 将日期转换为本日的结束时间
        /// </summary>
        /// <param name="value">任意时间</param>
        /// <returns>2001-01-01 23:59:59</returns>
        public static DateTime ToDayEnd(this DateTime value)
        {
            //返回日期加一天减一秒
            return value.Date.AddDays(1).AddSeconds(-1);
        }
        #endregion

        #region 周
        /// <summary>
        /// 将日期转换为本周的开始时间
        /// </summary>
        /// <param name="value">2001-01-01</param>
        /// <returns>2001-01-01 00:00:00</returns>
        public static DateTime ToWeekStart(string value)
        {
            //转换成日期类型
            var date = Convert.ToDateTime(value);
            return date.ToWeekStart();
        }

        /// <summary>
        /// 将日期转换为本周的开始时间
        /// </summary>
        /// <param name="value">任意时间</param>
        /// <returns>2001-01-01 00:00:00</returns>
        public static DateTime ToWeekStart(this DateTime value)
        {
            //根据当前时间取出该周周一的当前时间
            var weekStart = GetWeekStartOrEnd(value, true);
            return ToDayStart(weekStart);
        }

        /// <summary>
        /// 将日期转换为本周的结束时间
        /// </summary>
        /// <param name="value">2001-01-01</param>
        /// <returns>2001-01-01 23:59:59</returns>
        public static DateTime ToWeekEnd(string value)
        {
            //转换成日期类型
            var date = Convert.ToDateTime(value);
            return date.ToWeekEnd();
        }

        /// <summary>
        /// 将日期转换为本周的结束时间
        /// </summary>
        /// <param name="value">任意时间</param>
        /// <returns>2001-01-01 23:59:59</returns>
        public static DateTime ToWeekEnd(this DateTime value)
        {
            //根据当前时间取出该周周末的当前时间
            var weekEnd = GetWeekStartOrEnd(value, false);
            //返回日期加一天减一秒
            return ToDayEnd(weekEnd);
        }

        /// <summary>
        /// 获取本周的开始日期或结束日期
        /// </summary>
        /// <param name="date">用于计算的日期</param>
        /// <param name="getWeekStart">是否是获取一周开始的日期</param>
        /// <returns>日期</returns>
        private static DateTime GetWeekStartOrEnd(DateTime date, bool getWeekStart)
        {
            var weekStart = new DateTime();
            var startNum = getWeekStart ? 0 : 6;
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    weekStart = date.AddDays(startNum);
                    break;
                case DayOfWeek.Tuesday:
                    weekStart = date.AddDays(startNum - 1);
                    break;
                case DayOfWeek.Wednesday:
                    weekStart = date.AddDays(startNum - 2);
                    break;
                case DayOfWeek.Thursday:
                    weekStart = date.AddDays(startNum - 3);
                    break;
                case DayOfWeek.Friday:
                    weekStart = date.AddDays(startNum - 4);
                    break;
                case DayOfWeek.Saturday:
                    weekStart = date.AddDays(startNum - 5);
                    break;
                case DayOfWeek.Sunday:
                    weekStart = date.AddDays(startNum - 6);
                    break;
            }
            return weekStart;
        }

        #endregion

        #region 月
        /// <summary>
        /// 将日期转换为本月的开始时间
        /// </summary>
        /// <param name="value">2001-01-01</param>
        /// <returns>2001-01-01 00:00:00</returns>
        public static DateTime ToMonthStart(string value)
        {
            //转换成日期类型
            var date = Convert.ToDateTime(value);
            return date.ToMonthStart();
        }

        /// <summary>
        /// 将日期转换为本月的开始时间
        /// </summary>
        /// <param name="value">任意时间</param>
        /// <returns>2001-01-01 00:00:00</returns>
        public static DateTime ToMonthStart(this DateTime value)
        {
            //根据年、月重新创建日期
            return new DateTime(value.Year, value.Month, 1);
        }

        /// <summary>
        /// 将日期转换为本月的结束时间
        /// </summary>
        /// <param name="value">2001-01-01</param>
        /// <returns>2001-01-31 23:59:59</returns>
        public static DateTime ToMonthEnd(string value)
        {
            var date = ToMonthStart(value);
            return date.AddMonths(1).AddSeconds(-1);
        }

        /// <summary>
        /// 将日期转换为本月的结束时间
        /// </summary>
        /// <param name="value">任意时间</param>
        /// <returns>2001-01-31 23:59:59</returns>
        public static DateTime ToMonthEnd(this DateTime value)
        {
            var date = ToMonthStart(value);
            return date.AddMonths(1).AddSeconds(-1);
        }
        #endregion

        #region 年
        /// <summary>
        /// 将日期转换为本年的开始时间
        /// </summary>
        /// <param name="value">2001-01-01</param>
        /// <returns>2001-01-01 00:00:00</returns>
        public static DateTime ToYearStart(string value)
        {
            //转换成日期类型
            var date = Convert.ToDateTime(value);
            return date.ToYearStart();
        }

        /// <summary>
        /// 将日期转换为本年的开始时间
        /// </summary>
        /// <param name="value">任意时间</param>
        /// <returns>2001-01-01 00:00:00</returns>
        public static DateTime ToYearStart(this DateTime value)
        {
            //根据年、月重新创建日期 
            return new DateTime(value.Year, 1, 1);
        }

        /// <summary>
        /// 将日期转换为本年的结束时间
        /// </summary>
        /// <param name="value">2001-01-01</param>
        /// <returns>2001-12-31 23:59:59</returns>
        public static DateTime ToYearEnd(string value)
        {
            //转换成日期类型
            var date = Convert.ToDateTime(value);
            return date.ToYearEnd();
        }

        /// <summary>
        /// 将日期转换为本年的结束时间
        /// </summary>
        /// <param name="value">任意时间</param>
        /// <returns>2001-12-31 23:59:59</returns>
        public static DateTime ToYearEnd(this DateTime value)
        {
            //创建结束日期
            return new DateTime(value.Year + 1, 1, 1).AddSeconds(-1);
        }

        #endregion
    }
}
