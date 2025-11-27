using Domain.Common;

namespace Domain.Entities;

public class Role : BaseEntity
{
    // Id و CreatedAt از BaseEntity ارث‌بری می‌شوند

    public required string Name { get; set; }       // نام سیستمی (مثل: LabManager)
    public string? DisplayName { get; set; }        // نام فارسی (مثل: مدیر آزمایشگاه)
    public string? Description { get; set; }        // توضیحات دسترسی

    public bool IsActive { get; set; } = true;

    // ارتباط با کاربران (برای رابطه دوطرفه)
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}