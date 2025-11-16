using System;

namespace Shared.Icp.DTOs.QualityControl
{
    /// <summary>
    /// DTO تنظیمات کنترل کیفیت برای یک پروژه.
    /// مقادیر از ProjectSettings خوانده/نوشته می‌شوند.
    /// </summary>
    public class ProjectQualitySettingsDto
    {
        public Guid ProjectId { get; set; }

        public bool AutoQualityControl { get; set; }

        public double? MinAcceptableWeight { get; set; }
        public double? MaxAcceptableWeight { get; set; }

        public double? MinAcceptableVolume { get; set; }
        public double? MaxAcceptableVolume { get; set; }

        public int? MinDilutionFactor { get; set; }
        public int? MaxDilutionFactor { get; set; }
    }
}
