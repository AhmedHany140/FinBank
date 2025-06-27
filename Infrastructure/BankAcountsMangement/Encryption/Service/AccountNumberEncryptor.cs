using Infrastructure.BankAcountsMangement.Encryption.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.IO;

public class AccountNumberEncryptor : IAccountNumberEncryptor
{
	private readonly byte[] _keyBytes;
	private readonly byte[] _ivBytes;

	public AccountNumberEncryptor(IConfiguration configuration)
	{
		var section = configuration.GetSection("Encryption");
		var key = section["Key"];
		var iv = section["IV"];

		if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(iv))
			throw new ArgumentException("Encryption key or IV is not configured correctly.");

		_keyBytes = Convert.FromBase64String(key);
		_ivBytes = Convert.FromBase64String(iv);
	}

	public string Encrypt(string plainText)
	{
		using var aes = Aes.Create();
		aes.Key = _keyBytes;
		aes.IV = _ivBytes;

		using var encryptor = aes.CreateEncryptor();
		using var ms = new MemoryStream();
		using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
		using (var writer = new StreamWriter(cs))
		{
			writer.Write(plainText);
		}

		return Convert.ToBase64String(ms.ToArray());
	}

	public string Decrypt(string cipherText)
	{
		using var aes = Aes.Create();
		aes.Key = _keyBytes;
		aes.IV = _ivBytes;

		using var decryptor = aes.CreateDecryptor();
		using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
		using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
		using var reader = new StreamReader(cs);

		return reader.ReadToEnd();
	}
}
