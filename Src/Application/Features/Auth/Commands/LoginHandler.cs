using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Shared.Wrapper;
using System.Text;

namespace Application.Features.Auth.Commands.Login;

public class LoginHandler(
    IUnitOfWork unitOfWork,
    ITokenService tokenService
    ) : IRequestHandler<LoginCommand, Result<string>>
{
    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. پیدا کردن کاربر
        var users = await unitOfWork.Repository<User>()
            .GetAsync(u => u.UserName == request.UserName, includeProperties: "Roles");

        var user = users.FirstOrDefault();

        if (user == null)
            return await Result<string>.FailAsync("نام کاربری یا رمز عبور اشتباه است.");

        // 2. بررسی پسورد
        // نکته: باید دقیقاً از همان روش هشی استفاده کنید که در CreateUser استفاده کردید.
        // فعلاً فرض بر Simple Hash (Base64) است که در کدهای قبلی دیدیم.
        string inputHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(request.Password));

        if (user.PasswordHash != inputHash)
            return await Result<string>.FailAsync("نام کاربری یا رمز عبور اشتباه است.");

        if (!user.IsActive)
            return await Result<string>.FailAsync("حساب کاربری شما غیرفعال شده است.");

        // 3. تولید توکن
        var token = tokenService.GenerateToken(user);

        // ثبت زمان ورود
        user.LastLoginDate = DateTime.UtcNow;
        await unitOfWork.CommitAsync(cancellationToken);

        return await Result<string>.SuccessAsync(token, "ورود موفقیت‌آمیز بود.");
    }
}