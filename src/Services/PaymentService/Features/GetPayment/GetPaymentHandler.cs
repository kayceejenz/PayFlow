using System.Diagnostics;
using PaymentService.Domain;
using PaymentService.Infrastructure;
using PayFlow.Shared.Observability;

namespace PaymentService.Features.GetPayment;

public class GetPaymentHandler
{
    private readonly IPaymentRepository _repository;

    public GetPaymentHandler(IPaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GetPaymentResponse>> HandleAsync(GetPaymentQuery query, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("GetPayment");
        activity?.SetTag("payment.id", query.PaymentId);

        var payment = await _repository.GetByIdAsync(query.PaymentId, ct);

        if (payment == null)
            return Result.Failure<GetPaymentResponse>(PaymentErrors.NotFound);

        activity?.SetTag("payment.status", payment.Status.ToString());

        return Result.Success(new GetPaymentResponse
        {
            PaymentId = payment.Id,
            PayerAccountId = payment.PayerAccountId,
            MerchantAccountId = payment.MerchantAccountId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Status = payment.Status.ToString(),
            Reference = payment.Reference,
            FailureReason = payment.FailureReason,
            CreatedAtUtc = payment.CreatedAtUtc,
            UpdatedAtUtc = payment.UpdatedAtUtc
        });
    }
}
