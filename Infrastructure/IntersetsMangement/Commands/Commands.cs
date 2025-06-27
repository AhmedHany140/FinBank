using Infrastructure.ResultPattern;
using MediatR;

public record ApplyInterestCommand : IRequest<Result<bool>>;
public record CreateInterestRuleCommand(InterestRuleCreateDto Dto) : IRequest<Result<bool>>;