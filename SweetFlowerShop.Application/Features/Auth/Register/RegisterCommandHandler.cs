using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Features.Auth.Common;
using SweetFlowerShop.Application.Interfaces;

namespace SweetFlowerShop.Application.Features.Auth.Register;

public sealed class RegisterCommandHandler(IIdentityService identityService)
    : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await identityService.RegisterAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            cancellationToken);
    }
}
