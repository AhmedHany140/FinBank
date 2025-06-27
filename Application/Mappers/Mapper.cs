using Riok.Mapperly.Abstractions;
using Domain.Entities;
namespace Application.Mapping;

[Mapper]
public partial class Mapper
{
	public static Mapper Instance => new Mapper(); // Singleton use

	//  User
	public partial UserReadDto ToDto(ApplicationUser user);
	public partial ApplicationUser ToEntity(RegisterDto dto);

	//  BankAccount
	public partial BankAccountReadDto ToDto(BankAccount entity);
	public partial BankAccount ToEntity(BankAccountCreateDto dto);

	//  Transaction
	public partial TransactionReadDto ToDto(Transaction entity);
	public partial Transaction ToEntity(DepositDto dto);
	public partial Transaction ToEntity(WithdrawDto dto);

	public partial Transaction ToEntity(TransferDto dto);



	//  InterestRule
	public partial InterestRuleReadDto ToDto(InterestRule entity);
	public partial InterestRule ToEntity(InterestRuleCreateDto dto);

	//  ScheduledInterest
	public partial ScheduledInterestReadDto ToDto(ScheduledInterest entity);

	//  Notification
	public partial NotificationReadDto ToDto(Notification entity);
	public partial Notification ToEntity(NotificationCreateDto dto);


}
