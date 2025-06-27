using Infrastructure.ResultPattern;
using MediatR;

public record DepositCommand(DepositDto Dto) : IRequest<Result<bool>>;

public record WithdrawCommand(WithdrawDto Dto) : IRequest<Result<bool>>;
public record TransferCommand(TransferDto Dto) : IRequest<Result<bool>>;



