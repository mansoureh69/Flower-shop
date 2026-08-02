using FluentValidation;

namespace SweetFlowerShop.Application.Features.Orders.PlaceOrder;

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.Delivery)
            .NotNull().WithMessage("Delivery information is required.");

        When(x => x.Delivery is not null, () =>
        {
            RuleFor(x => x.Delivery.RecipientName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Delivery.RecipientPhone).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Delivery.Street).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Delivery.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Delivery.ZipCode).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Delivery.GiftMessage).MaximumLength(500);
        });

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}
