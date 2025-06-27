using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.NotificationsMangement.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.IntersetsMangement.AutoApplyIntersetRules.Services
{
	public class ApplyInterestService : IApplyInterestService
	{
		private readonly ApplicationDbContext _context;
		private readonly IMediator _mediator;

		public ApplyInterestService(ApplicationDbContext context, IMediator mediator)
		{
			_context = context;
			_mediator = mediator;
		}

		public async Task ApplyInterestAsync()
		{
			var rules = await _context.InterestRules
				.Where(r => r.Active)
				.ToListAsync();

			var today = DateTime.UtcNow;

			foreach (var rule in rules)
			{
				var accounts = await _context.BankAccounts
					.Where(a => a.Currency == rule.Currency && a.IsActive)
					.Include(a => a.User)
					.ToListAsync();

				foreach (var account in accounts)
				{
					var lastApplied = await _context.ScheduledInterests
						.Where(s => s.BankAccountId == account.Id && s.InterestRuleId == rule.Id)
						.OrderByDescending(s => s.AppliedAt)
						.Select(s => s.AppliedAt)
						.FirstOrDefaultAsync();

					if (!ShouldApplyInterest(rule.Compounding, lastApplied, today))
						continue;

					var interestAmount = account.Balance * rule.InterestRate;
					account.Balance += interestAmount;

					_context.ScheduledInterests.Add(new ScheduledInterest
					{
						Id = Guid.NewGuid(),
						BankAccountId = account.Id,
						InterestRuleId = rule.Id,
						Amount = interestAmount,
						AppliedAt = today
					});

					await _mediator.Publish(new BalanceChangedEvent(
						account.UserId,
						account.User.Email,
						interestAmount,
						account.Balance,
						reason: $"Interest Applied ({rule.Compounding})"
					));
				}
			}

			await _context.SaveChangesAsync();
		}

		private bool ShouldApplyInterest(CompoundingType type, DateTime lastApplied, DateTime now)
		{
			return type switch
			{
				CompoundingType.Daily => lastApplied.Date < now.Date,
				CompoundingType.Monthly => lastApplied.AddMonths(1).Date <= now.Date,
				CompoundingType.Yearly => lastApplied.AddYears(1).Date <= now.Date,
				_ => false,
			};
		}
	}
}
