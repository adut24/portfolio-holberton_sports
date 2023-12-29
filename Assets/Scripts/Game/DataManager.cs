using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

using UnityEngine;

public class DataManager : MonoBehaviour
{
	public string UserId { get; private set; }
	public Dictionary<string, (int, int)> MatchHistory { get; private set; }
	public Dictionary<string, int> HighScores { get; private set; }

	private string _filePath;
	private Data _data;

	private void Awake()
	{
		_filePath = Path.Combine(Application.persistentDataPath, "data.sav");
		_data = new Data();
		MatchHistory = new();
		HighScores = new();
	}
}

[Serializable]
public class Data
{
	public string id;
	public List<ScoreEntry> highScores;
	public List<MatchEntry> matchHistory;
}

[Serializable]
public class ScoreEntry
{
	public string sportName;
	public int score;
}

[Serializable]
public class MatchEntry
{
	public string opponentID;
	public (int, int) results;
}

public static class EncryptionSystem
{
	private static byte[] _key;

	static EncryptionSystem()
	{
		string path = Path.Combine(Application.persistentDataPath, "BlueHarmony.dat");
		if (File.Exists(path))
		{
			if (new FileInfo(path).Length > 0)
				_key = File.ReadAllBytes(path);
			else
				_key = GenerateRandomAesKey();
		}
		else
			_key = GenerateRandomAesKey();
	}

	public static string EncryptAES(string input)
	{
		using Aes aes = Aes.Create();
		aes.Key = _key;
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
			aes.Key = _key;
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

	private static byte[] GenerateRandomAesKey()
	{
		int[] keyLengths = { 128, 192, 256 };

		int selectedKeyLength = keyLengths[new System.Random().Next(keyLengths.Length)];

		byte[] key = new byte[selectedKeyLength / 8];
		using RNGCryptoServiceProvider rng = new();
		rng.GetBytes(key);

		return key;
	}
}
