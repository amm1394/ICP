using MediatR;
using Shared.Wrapper;

namespace Application.Features.Auth.Commands.Login;

public record LoginCommand(string UserName, string Password) : IRequest<Result<string>>;