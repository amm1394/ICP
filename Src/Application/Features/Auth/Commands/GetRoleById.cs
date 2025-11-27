using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Shared.Wrapper;

namespace Application.Features.Roles.Queries.GetRoleById;

public record GetRoleByIdQuery(Guid Id) : IRequest<Result<Role>>;

public class GetRoleByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetRoleByIdQuery, Result<Role>>
{
    public async Task<Result<Role>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await unitOfWork.Repository<Role>().GetByIdAsync(request.Id);

        if (role == null)
            return await Result<Role>.FailAsync("نقش یافت نشد.");

        return await Result<Role>.SuccessAsync(role);
    }
}