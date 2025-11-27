using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Shared.Wrapper;

namespace Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(User User, string? NewPassword = null) : IRequest<Result<Guid>>;

public class UpdateUserCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // لود کردن کاربر همراه با نقش‌ها برای آپدیت صحیح
        var users = await unitOfWork.Repository<User>().GetAsync(u => u.Id == request.User.Id, "Roles");
        var user = users.FirstOrDefault();

        if (user == null)
            return await Result<Guid>.FailAsync("کاربر یافت نشد.");

        // آپدیت اطلاعات پایه
        user.FullName = request.User.FullName;
        user.Email = request.User.Email;
        user.Position = request.User.Position;
        user.PhoneNumber = request.User.PhoneNumber;
        user.IsActive = request.User.IsActive;

        // تغییر رمز عبور در صورت ارسال
        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            user.PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(request.NewPassword));
        }

        // آپدیت نقش‌ها (حذف قبلی‌ها و افزودن جدیدها)
        user.Roles.Clear();
        if (request.User.Roles != null && request.User.Roles.Any())
        {
            foreach (var r in request.User.Roles)
            {
                var roleDb = (await unitOfWork.Repository<Role>().GetAsync(x => x.Name == r.Name)).FirstOrDefault();
                if (roleDb != null) user.Roles.Add(roleDb);
            }
        }

        await unitOfWork.Repository<User>().UpdateAsync(user);
        await unitOfWork.CommitAsync(cancellationToken);

        return await Result<Guid>.SuccessAsync(user.Id, "کاربر ویرایش شد.");
    }
}