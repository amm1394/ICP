using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Shared.Wrapper;

namespace Application.Features.Roles.Commands.CreateRole;

public record CreateRoleCommand(Role RoleDto) : IRequest<Result<Guid>>;

public class CreateRoleCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateRoleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // بررسی تکراری نبودن نام نقش
        var existingRoles = await unitOfWork.Repository<Role>()
            .GetAsync(r => r.Name == request.RoleDto.Name);

        if (existingRoles.Any())
        {
            return await Result<Guid>.FailAsync("نقشی با این نام قبلاً وجود دارد.");
        }

        var role = new Role
        {
            Name = request.RoleDto.Name,
            DisplayName = request.RoleDto.DisplayName,
            Description = request.RoleDto.Description,
            IsActive = true
        };

        await unitOfWork.Repository<Role>().AddAsync(role);
        await unitOfWork.CommitAsync(cancellationToken);

        return await Result<Guid>.SuccessAsync(role.Id, "نقش جدید با موفقیت ایجاد شد.");
    }
}