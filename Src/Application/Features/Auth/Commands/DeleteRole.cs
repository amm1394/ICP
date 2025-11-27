using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Shared.Wrapper;

namespace Application.Features.Roles.Commands.DeleteRole;

public record DeleteRoleCommand(Guid Id) : IRequest<Result<Guid>>;

public class DeleteRoleCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteRoleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await unitOfWork.Repository<Role>().GetByIdAsync(request.Id);

        if (role == null)
            return await Result<Guid>.FailAsync("نقش یافت نشد.");

        await unitOfWork.Repository<Role>().DeleteAsync(role);
        await unitOfWork.CommitAsync(cancellationToken);

        return await Result<Guid>.SuccessAsync(role.Id, "نقش حذف شد.");
    }
}