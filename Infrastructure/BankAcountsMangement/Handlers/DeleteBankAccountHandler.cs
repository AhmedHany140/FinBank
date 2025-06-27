using Application.Mapping;
using Infrastructure.ResultPattern;
using Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace Infrastructure.BankAcountsMangement.Handulers
{
	public class DeleteBankAccountHandler : IRequestHandler<DeleteBankAcountCommand, Result<bool>>
	{
		private readonly IUnitOfWork _unitOfWork;
	

		public DeleteBankAccountHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
			
		}

		public async Task<Result<bool>> Handle(DeleteBankAcountCommand request, CancellationToken cancellationToken)
		{
			var id = request.Id;
			if (id == Guid.Empty)
			{
				return Result<bool>.Failure(new Error("InvalidId", "The provided ID is invalid."));
			}
		
			await _unitOfWork.BankAccounts.DeleteAsync(id);
		    await _unitOfWork.CompleteAsync();

			return Result<bool>.Success(true);

		}
	}



}
