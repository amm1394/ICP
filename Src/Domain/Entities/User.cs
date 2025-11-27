using Domain.Common;

namespace Domain.Entities;

public class User : BaseEntity
{
    // Id و CreatedAt از BaseEntity ارث‌بری می‌شوند

    public required string UserName { get; set; } // نام کاربری (Unique)
    public required string FullName { get; set; } // نام نمایشی
    public required string Email { get; set; }    // ایمیل سازمانی (Unique)

    public string? Position { get; set; }         // سمت شغلی
    public string? PhoneNumber { get; set; }      // شماره تماس

    public string PasswordHash { get; set; } = string.Empty; // ذخیره پسورد هش شده

    public bool IsActive { get; set; } = true;    // وضعیت کاربر
    public DateTime? LastLoginDate { get; set; }  // آخرین زمان ورود

    // ارتباط چند به چند با نقش‌ها
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}