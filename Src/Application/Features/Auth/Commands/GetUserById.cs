using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Shared.Wrapper;

namespace Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IRequest<Result<User>>;

public class GetUserByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetUserByIdQuery, Result<User>>
{
    public async Task<Result<User>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var users = await unitOfWork.Repository<User>().GetAsync(u => u.Id == request.Id, "Roles");
        var user = users.FirstOrDefault();

        if (user == null)
            return await Result<User>.FailAsync("کاربر یافت نشد.");

        return await Result<User>.SuccessAsync(user);
    }
}