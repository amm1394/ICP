using System;

namespace Core.Icp.Domain.Models.QualityControl
{
    /// <summary>
    /// نتیجه یک QC برای یک نمونه و یک نوع چک مشخص.
    /// </summary>
    public class QualityCheckResult
    {
        public Guid ProjectId { get; set; }
        public Guid SampleId { get; set; }

        /// <summary>
        /// نوع QC (مثلاً "Weight", "Volume", "DF", "Empty", "CRM", "RM").
        /// </summary>
        public string CheckType { get; set; } = string.Empty;

        /// <summary>
        /// وضعیت نهایی چک (مثلاً "Passed", "Warning", "Failed", "NotImplemented").
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// پیام توضیحی برای کاربر (در صورت نیاز).
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
