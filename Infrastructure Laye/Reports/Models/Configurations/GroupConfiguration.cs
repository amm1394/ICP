using Core.Icp.Domain.Enums;

namespace Infrastructure.Icp.Reports.Models.Configurations
{
    /// <summary>
    /// تنظیمات گروه‌بندی
    /// </summary>
    public class GroupConfiguration
    {
        /// <summary>
        /// نوع گروه‌بندی
        /// </summary>
        public GroupType Type { get; set; }

        /// <summary>
        /// مقدار شروع
        /// </summary>
        public double? StartValue { get; set; }

        /// <summary>
        /// مقدار پایان
        /// </summary>
        public double? EndValue { get; set; }

        /// <summary>
        /// اندازه فاصله
        /// </summary>
        public double? Interval { get; set; }
    }
}