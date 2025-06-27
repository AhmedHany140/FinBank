using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
namespace Domain.Entities
{
	public class ApplicationUser : IdentityUser<Guid>
	{
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
		public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
	}

}
