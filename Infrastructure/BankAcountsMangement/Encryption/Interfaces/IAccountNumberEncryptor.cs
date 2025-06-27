using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.BankAcountsMangement.Encryption.Interfaces
{
	public interface IAccountNumberEncryptor
	{
		string Encrypt(string plainText);
		string Decrypt(string cipherText);
	}
}
