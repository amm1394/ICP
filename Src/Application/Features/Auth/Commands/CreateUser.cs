using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Shared.Wrapper;

namespace Application.Features.Users.Commands.CreateUser;

public class CreateUserCommand : IRequest<Result<Guid>>
{
    public required string UserName { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
    public string? Position { get; set; }
    public string? PhoneNumber { get; set; }

    // لیستی از نام نقش‌ها برای انتساب به کاربر (اختیاری)
    public List<string>? Roles { get; set; }
}

public class CreateUserCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. اعتبارسنجی اولیه
        if (request.Password != request.ConfirmPassword)
        {
            return await Result<Guid>.FailAsync("کلمه عبور و تکرار آن مطابقت ندارند.");
        }

        // 2. بررسی تکراری نبودن نام کاربری یا ایمیل
        var existingUser = await unitOfWork.Repository<User>()
            .GetAsync(u => u.UserName == request.UserName || u.Email == request.Email);

        if (existingUser.Any())
        {
            return await Result<Guid>.FailAsync("کاربری با این نام کاربری یا ایمیل قبلاً ثبت شده است.");
        }

        // 3. ساخت موجودیت کاربر
        var user = new User
        {
            UserName = request.UserName,
            FullName = request.FullName,
            Email = request.Email,
            Position = request.Position,
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            LastLoginDate = null,
            // نکته: در محیط واقعی حتماً از سرویس هش پسورد استفاده کنید
            // فعلاً برای کامپایل شدن، پسورد را مستقیم یا با هش ساده می‌ریزیم
            PasswordHash = HashPassword(request.Password)
        };

        // 4. انتساب نقش‌ها (در صورت وجود)
        if (request.Roles != null && request.Roles.Any())
        {
            var roles = await unitOfWork.Repository<Role>()
                .GetAsync(r => request.Roles.Contains(r.Name));

            foreach (var role in roles)
            {
                user.Roles.Add(role);
            }
        }

        // 5. ذخیره در دیتابیس
        await unitOfWork.Repository<User>().AddAsync(user);
        await unitOfWork.CommitAsync(cancellationToken);

        return await Result<Guid>.SuccessAsync(user.Id, "کاربر با موفقیت ایجاد شد.");
    }

    // متد کمکی موقت برای هش کردن (در آینده با IPasswordHasher جایگزین کنید)
    private static string HashPassword(string password)
    {
        // TODO: Replace with secure hashing (e.g., BCrypt, Argon2)
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
    }
}