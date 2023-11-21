using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using UnityEngine;

[Serializable]
public class DataManager : MonoBehaviour
{
	public string UserId { get; private set; }
	public Dictionary<string, (int, int)> MatchHistory { get; set; }
	public Dictionary<string, int> HighScores { get; set; }
	private string _filePath;

	public DataManager()
	{
		_filePath = Path.Combine(Application.persistentDataPath, "data.sav");
	}

	public string ToJson() => JsonUtility.ToJson(this);
}

public static class EncryptionSystem
{
	private static string _key;

	static EncryptionSystem()
	{

	}

	public static string EncryptAES(string input)
	{
		using Aes aes = Aes.Create();
		aes.Key = Encoding.UTF8.GetBytes(_key);
		aes.IV = new byte[16];

		ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

		using MemoryStream msEncrypt = new();
		using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
		using StreamWriter swEncrypt = new(csEncrypt);
		swEncrypt.Write(input);

		return Convert.ToBase64String(msEncrypt.ToArray());
	}

	public static string DecryptAES(string encryptedData)
	{
		try
		{
			using Aes aes = Aes.Create();
			aes.Key = Encoding.UTF8.GetBytes(_key);
			aes.IV = new byte[16];

			ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

			using MemoryStream msDecrypt = new(Convert.FromBase64String(encryptedData));
			using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
			using StreamReader srDecrypt = new(csDecrypt);
			return srDecrypt.ReadToEnd();
		}
		catch
		{
			return null;
		}
	}
}
