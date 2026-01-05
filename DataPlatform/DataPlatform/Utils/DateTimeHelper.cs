using System;
namespace DataPlatform.Utils
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// 将可空 DateTime 安全转换成本地时间
        /// </summary>
        /// <param name="dt">原始可空 DateTime（可为 Utc/Local/Unspecified）</param>
        /// <param name="timeZoneId">
        /// 可选：目标时区 ID，例如 "China Standard Time"。
        /// 不传则使用系统本地时区。
        /// </param>
        /// <returns>转换后的本地时间或 null</returns>

        public static DateTime? ToSafeLocalTime(DateTime? dt, string? timeZoneId = null)
        {
            if (!dt.HasValue)
                return null;

            DateTime utcTime = dt.Value.Kind switch
            {
                DateTimeKind.Utc => dt.Value,
                DateTimeKind.Local => dt.Value.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc),
                _ => throw new ArgumentOutOfRangeException(nameof(dt.Value.Kind), "未知 DateTimeKind")
            };

            TimeZoneInfo tz = string.IsNullOrEmpty(timeZoneId)
                ? TimeZoneInfo.Local
                : TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
        }

    }

}
