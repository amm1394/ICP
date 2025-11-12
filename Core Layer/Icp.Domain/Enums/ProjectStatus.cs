namespace Core.Icp.Domain.Enums
{
    public enum ProjectStatus
    {
        /// <summary>
        /// پروژه جدید ایجاد شده
        /// </summary>
        Created = 0,

        /// <summary>
        /// در حال پردازش
        /// </summary>
        Processing = 1,

        /// <summary>
        /// پردازش کامل شده
        /// </summary>
        Processed = 2,

        /// <summary>
        /// در حال بررسی کیفیت
        /// </summary>
        UnderQualityCheck = 3,

        /// <summary>
        /// تایید شده
        /// </summary>
        Approved = 4,

        /// <summary>
        /// رد شده
        /// </summary>
        Rejected = 5,

        /// <summary>
        /// بایگانی شده
        /// </summary>
        Archived = 6,

        /// <summary>
        /// در حال ویرایش
        /// </summary>
        Draft = 7
    }
}