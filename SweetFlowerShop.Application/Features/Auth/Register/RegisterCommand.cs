using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Features.Auth.Common;

namespace SweetFlowerShop.Application.Features.Auth.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName) : IRequest<Result<AuthResponse>>;
