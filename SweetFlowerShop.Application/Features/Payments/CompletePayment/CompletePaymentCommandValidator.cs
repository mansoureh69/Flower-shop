using FluentValidation;

namespace SweetFlowerShop.Application.Features.Payments.CompletePayment;

public sealed class CompletePaymentCommandValidator : AbstractValidator<CompletePaymentCommand>
{
    public CompletePaymentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ProviderTransactionId).NotEmpty().MaximumLength(200);
    }
}
