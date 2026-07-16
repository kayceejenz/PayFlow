using StatementService.Infrastructure;

namespace StatementService.Features.GetStatements;

public class GetStatementsHandler
{
    private readonly IStatementRepository _repository;

    public GetStatementsHandler(IStatementRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetStatementsResponse> HandleAsync(
        GetStatementsQuery query, CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var skip = (page - 1) * pageSize;

        var totalCount = await _repository.GetTotalCountAsync(query.WalletId, ct);
        var entries = await _repository.GetPagedAsync(query.WalletId, skip, pageSize, ct);

        return new GetStatementsResponse(
            query.WalletId,
            entries.Select(e => new StatementEntryResponse(
                e.Id,
                e.TransactionId,
                e.EntryType,
                e.Amount,
                e.Currency,
                e.CounterpartyId,
                e.Reference,
                e.CreatedAtUtc)).ToList(),
            page,
            pageSize,
            totalCount);
    }
}
