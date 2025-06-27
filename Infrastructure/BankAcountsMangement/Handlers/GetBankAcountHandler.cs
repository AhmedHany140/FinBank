using Application.Mapping;
using Infrastructure.ResultPattern;
using Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.BankAcountsMangement.Handulers
{
	public class GetBankAcountHandler:IRequestHandler<GetBankAcountQuery,Result<BankAccountReadDto>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly Mapper mapper;
		public GetBankAcountHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
			mapper = Mapper.Instance;
		}

		public async Task<Result<BankAccountReadDto>> Handle(GetBankAcountQuery request, CancellationToken cancellationToken)
		{
			var id = request.Id;
			if (id == Guid.Empty)
			{
				return Result<BankAccountReadDto>.Failure(new Error("InvalidId", "The provided ID is invalid."));
			}
			var bankAccount = await _unitOfWork.BankAccounts.GetByIdAsync(id);
			if (bankAccount == null)
			{
				return Result<BankAccountReadDto>.Failure(new Error("NotFound", "Bank account not found."));
			}
			var dto = mapper.ToDto(bankAccount);
			return Result<BankAccountReadDto>.Success(dto);
		}
	}
}
