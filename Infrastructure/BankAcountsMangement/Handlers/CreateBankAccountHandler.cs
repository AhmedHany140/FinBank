using Application.Mapping;
using Infrastructure.BankAcountsMangement.Encryption.Interfaces;
using Infrastructure.Data;
using Infrastructure.NotificationsMangement.Entity;
using Infrastructure.ResultPattern;
using Infrastructure.UnitOfWork.Interfaces;
using MediatR;


namespace Infrastructure.BankAcountsMangement.Handulers
{
	
	public class CreateBankAccountHandler : IRequestHandler<CreateBankAccountCommand,Result<bool>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly Mapper _mapper;
		private readonly IMediator _mediator;
		private readonly ApplicationDbContext _dbContext;
		private readonly IAccountNumberEncryptor _accountNumberEncryptor;

		public CreateBankAccountHandler(IUnitOfWork unitOfWork,IAccountNumberEncryptor encryptor,ApplicationDbContext dbContext, IMediator mediator)
		{
			_unitOfWork = unitOfWork;
			_mapper = Mapper.Instance;
			_mediator = mediator;
			_dbContext = dbContext;
			_accountNumberEncryptor = encryptor;
		}

		public async Task<Result<bool>> Handle(CreateBankAccountCommand request, CancellationToken cancellationToken)
		{
			var dto = request.Dto;

			var entity = _mapper.ToEntity(dto);


			entity.Id = Guid.NewGuid();

			await _unitOfWork.BankAccounts.AddAsync(entity);
		    await _unitOfWork.CompleteAsync();

			var user = await _dbContext.Users.FindAsync(dto.UserId);

			var decrptAcountnumber= _accountNumberEncryptor.Decrypt(entity.AccountNumber);

			await _mediator.Publish(new NewBankAcountEvent(
			userId: entity.UserId,
			 acountNumber: decrptAcountnumber,
			email: user.Email,
			balance: dto.Balance 
			));

			return Result<bool>.Success(true);
		}
	}

}
