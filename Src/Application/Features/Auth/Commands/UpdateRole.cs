using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Shared.Wrapper;

namespace Application.Features.Roles.Commands.UpdateRole;

public record UpdateRoleCommand(Role Role) : IRequest<Result<Guid>>;

public class UpdateRoleCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateRoleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await unitOfWork.Repository<Role>().GetByIdAsync(request.Role.Id);

        if (role == null)
            return await Result<Guid>.FailAsync("نقش یافت نشد.");

        // آپدیت فیلدها
        role.Name = request.Role.Name;
        role.DisplayName = request.Role.DisplayName;
        role.Description = request.Role.Description;
        role.IsActive = request.Role.IsActive;

        await unitOfWork.Repository<Role>().UpdateAsync(role);
        await unitOfWork.CommitAsync(cancellationToken);

        return await Result<Guid>.SuccessAsync(role.Id, "نقش ویرایش شد.");
    }
}