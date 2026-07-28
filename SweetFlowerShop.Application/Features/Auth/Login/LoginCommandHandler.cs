using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Features.Auth.Common;
using SweetFlowerShop.Application.Interfaces;

namespace SweetFlowerShop.Application.Features.Auth.Login;

public sealed class LoginCommandHandler(IIdentityService identityService)
    : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await identityService.LoginAsync(request.Email, request.Password, cancellationToken);
    }
}
