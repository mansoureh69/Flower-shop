using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Features.Auth.Common;

namespace SweetFlowerShop.Application.Features.Auth.Login;

public record LoginCommand(
    string Email,
    string Password) : IRequest<Result<AuthResponse>>;
