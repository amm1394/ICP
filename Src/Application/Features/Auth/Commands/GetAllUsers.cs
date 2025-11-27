using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Shared.Wrapper;

namespace Application.Features.Users.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<Result<List<User>>>;

public class GetAllUsersQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllUsersQuery, Result<List<User>>>
{
    public async Task<Result<List<User>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await unitOfWork.Repository<User>().GetAsync(null, "Roles");

        var userList = users.Select(u => new User
        {
            Id = u.Id,
            UserName = u.UserName,
            FullName = u.FullName,
            Email = u.Email,
            Position = u.Position,
            PhoneNumber = u.PhoneNumber,
            IsActive = u.IsActive,
            LastLoginDate = u.LastLoginDate ?? DateTime.MinValue,
            Roles = u.Roles.ToList()
        }).ToList();

        return await Result<List<User>>.SuccessAsync(userList);
    }
}