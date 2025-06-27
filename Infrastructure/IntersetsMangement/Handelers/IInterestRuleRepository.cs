using Application.Loging;
using Application.Mapping;
using Infrastructure.ResultPattern;
using Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.IntersetsMangement.Handelers
{
	public class IInterestRuleRepository : IRequestHandler<CreateInterestRuleCommand, Result<bool>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly Mapper _mapper;
		public IInterestRuleRepository(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
			_mapper = Mapper.Instance;
		}
		public async Task<Result<bool>> Handle(CreateInterestRuleCommand request, CancellationToken cancellationToken)
		{
		   var dto = request.Dto;

			if (dto == null)
			{
				LogExceptions.LogEx(new ArgumentNullException(nameof(dto)), "CreateInterestRuleHandler.Handle");
				return Result<bool>.Failure(new Error("CreateInterestRuleHandler", "Dto can't be null"));
			}
			var entity = _mapper.ToEntity(dto);
			if (entity == null)
			{
				LogExceptions.LogEx(new ArgumentNullException(nameof(entity)), "CreateInterestRuleHandler.Handle");
				return Result<bool>.Failure(new Error("CreateInterestRuleHandler", "Failed to map DTO to entity"));
			}
			entity.Id = Guid.NewGuid();
			await _unitOfWork.Rules.AddAsync(entity);
			var result = await _unitOfWork.CompleteAsync();
			if (result <= 0)
			{
				LogExceptions.LogEx(new Exception("Database error"), "CreateInterestRuleHandler.Handle");
				return Result<bool>.Failure(new Error("DatabaseError", "Failed to create interest rule."));
			}
			return Result<bool>.Success(true);
		}
	}
}
