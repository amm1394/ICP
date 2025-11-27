using Domain.Common;
using Domain.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Domain.Entities;

public class Project : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    // نام فایل اصلی که ایمپورت شده (طبق مستندات)
    public string? SourceFileName { get; set; }

    // فیلد ذخیره تنظیمات در دیتابیس (JSON)
    public string? SettingsJson { get; set; }

    // --- Navigation Properties ---

    // لیست نمونه‌ها
    public virtual ICollection<Sample> Samples { get; set; } = new List<Sample>();

    // لیست منحنی‌های کالیبراسیون (اضافه شده برای فاز ۴)
    public virtual ICollection<CalibrationCurve> CalibrationCurves { get; set; } = new List<CalibrationCurve>();

    // --- Helper Properties ---

    // این پراپرتی در دیتابیس ذخیره نمی‌شود، بلکه رابطی روی SettingsJson است
    // باعث می‌شود در کد به جای کار با رشته، با کلاس ProjectSettings کار کنید.
    [NotMapped]
    public ProjectSettings Settings
    {
        get
        {
            if (string.IsNullOrWhiteSpace(SettingsJson))
                return new ProjectSettings();

            try
            {
                return JsonSerializer.Deserialize<ProjectSettings>(SettingsJson) ?? new ProjectSettings();
            }
            catch
            {
                // در صورت خرابی جیسون، تنظیمات پیش‌فرض برگردان
                return new ProjectSettings();
            }
        }
        set
        {
            SettingsJson = JsonSerializer.Serialize(value);
        }
    }
}