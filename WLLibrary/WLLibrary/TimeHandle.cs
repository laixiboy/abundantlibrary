using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WLLibrary
{
    public class TimeHandle
    {
        public readonly static DateTime WORLD_BEGINTIME = new DateTime(1970, 1, 1, 0, 0, 0);

        /// <summary>
        /// @brief:将北京时间转化为格林尼治为基准的时间戳(单位毫秒)
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A Json string representation of the <see cref="DateTime"/>.</returns>
        public static long ConvertDatetimeToMS(DateTime value)
        {
            return (value.ToUniversalTime().Ticks - TimeHandle.WORLD_BEGINTIME.Ticks) / 10000L;
        }
        /// <summary>
        /// @brief:将北京时间转化为格林尼治为基准的时间戳(单位秒)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static UInt32 ConvertDatetimeToSec(DateTime value)
        {
            return (UInt32)((value.ToUniversalTime().Ticks - TimeHandle.WORLD_BEGINTIME.Ticks) / 10000000L);
        }

        /// <summary>
        /// @brief:将格林尼治时间为基准的时间戳(单位毫秒)转化为北京时间
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public static DateTime ConvertMSToDateTime(long microSecond)
        {
            DateTime dateStart = TimeZone.CurrentTimeZone.ToLocalTime(TimeHandle.WORLD_BEGINTIME);
            return dateStart.AddMilliseconds(microSecond);
        }
        /// <summary>
        /// @brief:将格林尼治时间为基准的时间戳(单位秒)转化为北京时间
        /// </summary>
        /// <param name="second"></param>
        /// <returns></returns>
        public static DateTime ConvertSecToDateTime(UInt32 second)
        {
            double value = 1000.0 * second;
            DateTime dateStart = TimeZone.CurrentTimeZone.ToLocalTime(TimeHandle.WORLD_BEGINTIME);
            return dateStart.AddMilliseconds(value);
        }
    }
}
