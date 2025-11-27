using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Shared.Wrapper;

namespace Application.Features.Roles.Queries.GetAllRoles;

public record GetAllRolesQuery : IRequest<Result<List<Role>>>;

public class GetAllRolesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllRolesQuery, Result<List<Role>>>
{
    public async Task<Result<List<Role>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        // مشابه GetAllUsers: لود با include
        var roles = await unitOfWork.Repository<Role>().GetAsync(null, "Users");

        // Projection مثل GetAllUsers
        var roleList = roles.Select(r => new Role
        {
            Id = r.Id,
            Name = r.Name,
            DisplayName = r.DisplayName,
            Description = r.Description,
            IsActive = r.IsActive,
            Users = r.Users.ToList()
        }).ToList();

        return await Result<List<Role>>.SuccessAsync(roleList);
    }
}
