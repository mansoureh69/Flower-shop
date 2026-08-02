using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Domain.Entities;
using SweetFlowerShop.Domain.Enums;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Application.Features.Payments.CompletePayment;

/// <summary>
/// Atomic persistence step for a payment already verified by a provider adapter.
/// Deliberately not exposed by the customer-facing API.
/// </summary>
public sealed class CompletePaymentCommandHandler(
    IOrderRepository orders,
    IPaymentRepository payments,
    IUnitOfWork unitOfWork) : IRequestHandler<CompletePaymentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CompletePaymentCommand request, CancellationToken cancellationToken)
    {
        Result<Guid>? result = null;
        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var order = await orders.GetByIdAsync(request.OrderId, ct);
            if (order is null)
            {
                result = Result<Guid>.Failure("Order not found.");
                return;
            }

            if (order.Status != OrderStatus.PendingPayment)
            {
                result = Result<Guid>.Failure("Order is not awaiting payment.");
                return;
            }

            if (await payments.GetByOrderIdAsync(order.Id, ct) is not null)
            {
                result = Result<Guid>.Failure("A payment already exists for this order.");
                return;
            }

            var currency = order.Items.Select(item => item.UnitPrice.Currency).Distinct().Single();
            var payment = new Payment(order.Id, new Money(order.TotalAmount, currency), request.Method);
            payment.MarkAsPaid(request.ProviderTransactionId);
            order.ConfirmPayment();

            await payments.AddAsync(payment, ct);
            orders.Update(order);
            result = Result<Guid>.Success(payment.Id);
        }, cancellationToken);

        return result ?? Result<Guid>.Failure("Payment could not be completed.");
    }
}
