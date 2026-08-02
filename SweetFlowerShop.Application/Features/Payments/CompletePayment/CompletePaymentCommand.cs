using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Domain.Enums;

namespace SweetFlowerShop.Application.Features.Payments.CompletePayment;

public sealed record CompletePaymentCommand(Guid OrderId, PaymentMethod Method, string ProviderTransactionId)
    : IRequest<Result<Guid>>;
