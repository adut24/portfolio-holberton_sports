using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using UnityEngine;

/// <summary>
/// 
/// </summary>
public class DataManager : MonoBehaviour
{
	/// <summary>
	/// 
	/// </summary>
	public string UserID { get; private set; }

	/// <summary>
	/// 
	/// </summary>
	public Dictionary<string, (int, int)> MatchHistory { get; set; }

	/// <summary>
	/// 
	/// </summary>
	public Dictionary<string, int> HighScores { get; set; }

	private string _filePath;
	private Data _data;

	/// <summary>
	/// 
	/// </summary>
	private void Awake()
	{
		_filePath = Path.Combine(Application.persistentDataPath, "data.sav");
		MatchHistory = new();
		HighScores = new();
		if (File.Exists(_filePath))
		{
			string[] fileContents = Encoding.Default.GetString(EncryptionSystem.ParseHexadecimalString(File.ReadAllText(_filePath))).Split("|!$*|000", StringSplitOptions.None);
			if (fileContents.Length == 4 
				&& fileContents[1] == EncryptionSystem.CalculateChecksum(fileContents[0])
				&& fileContents[3] == EncryptionSystem.CalculateChecksum(fileContents[2]))
				LoadData(fileContents[0]);
			else
			{
				HighScores.Add("Bowling", 0);
				HighScores.Add("Archery", 0);
				UserID = Guid.NewGuid().ToString();
				//SaveData();
			}
		}
		else
		{
			HighScores.Add("Bowling", 0);
			HighScores.Add("Archery", 0);
			UserID = Guid.NewGuid().ToString();
			//SaveData();
		}
		Debug.Log($"User ID : {UserID}");
	}

	/// <summary>
	/// 
	/// </summary>
	public void SaveData()
	{
		_data = new() { id = UserID };
		foreach (KeyValuePair<string, int> kvp in HighScores)
			_data.highScores.Add(new ScoreEntry { sportName = kvp.Key, score = kvp.Value });
		foreach (KeyValuePair<string, (int, int)> kvp in MatchHistory)
			_data.matchHistory.Add(new MatchEntry { opponentID = kvp.Key, results = kvp.Value });

		string encryptedData = EncryptionSystem.EncryptAES(JsonUtility.ToJson(_data));
		string dataChecksum = EncryptionSystem.CalculateChecksum(encryptedData);
		string keyAsBase64 = Convert.ToBase64String(EncryptionSystem.Key);
		string keyChecksum = EncryptionSystem.CalculateChecksum(keyAsBase64);
		string combinedData = encryptedData + "|!$*|000" + dataChecksum + "|!$*|000" + keyAsBase64 + "|!$*|000" + keyChecksum;
		File.WriteAllText(_filePath, EncryptionSystem.FormatHexadecimalString(Encoding.Default.GetBytes(combinedData)));
	}

	private void LoadData(string fileContent)
	{
		_data = JsonUtility.FromJson<Data>(EncryptionSystem.DecryptAES(fileContent));
		UserID = _data.id;
		foreach (ScoreEntry highScore in _data.highScores)
			HighScores.Add(highScore.sportName, highScore.score);
		foreach (MatchEntry opposition in _data.matchHistory)
			MatchHistory.Add(opposition.opponentID, opposition.results);
	}
}

/// <summary>
/// 
/// </summary>
[Serializable]
public class Data
{
	/// <summary>
	/// 
	/// </summary>
	public string id;

	/// <summary>
	/// 
	/// </summary>
	public List<ScoreEntry> highScores;

	/// <summary>
	/// 
	/// </summary>
	public List<MatchEntry> matchHistory;

	/// <summary>
	/// 
	/// </summary>
	public Data()
	{
		highScores = new();
		matchHistory = new();
	}
}

/// <summary>
/// 
/// </summary>
[Serializable]
public class ScoreEntry
{
	/// <summary>
	/// 
	/// </summary>
	public string sportName;

	/// <summary>
	/// 
	/// </summary>
	public int score;
}

/// <summary>
/// 
/// </summary>
[Serializable]
public class MatchEntry
{
	/// <summary>
	/// 
	/// </summary>
	public string opponentID;
	/// <summary>
	/// 
	/// </summary>
	public (int, int) results;
}

/// <summary>
/// 
/// </summary>
public static class EncryptionSystem
{
	/// <summary>
	/// 
	/// </summary>
	public static byte[] Key { get => _key; }

	private static readonly byte[] _key;

	/// <summary>
	/// 
	/// </summary>
	static EncryptionSystem()
	{
		string path = Path.Combine(Application.persistentDataPath, "data.sav");
		if (File.Exists(path))
		{
			string[] fileContents = Encoding.Default.GetString(ParseHexadecimalString(File.ReadAllText(path))).Split("|!$*|000", StringSplitOptions.None);
			if (fileContents.Length == 4 && fileContents[3] == CalculateChecksum(fileContents[2]))
				_key = Convert.FromBase64String(fileContents[2]);
			else
				_key = GenerateRandomAesKey();
		}
		else
			_key = GenerateRandomAesKey();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static string EncryptAES(string input)
	{
		using Aes aes = Aes.Create();
		aes.Key = Key;
		aes.GenerateIV();

		ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

		using MemoryStream msEncrypt = new();
		using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
		using StreamWriter swEncrypt = new(csEncrypt);

		swEncrypt.Write(input);
		swEncrypt.Close();
		csEncrypt.Close();

		byte[] iv = aes.IV;
		byte[] encrypted = msEncrypt.ToArray();
		byte[] result = new byte[iv.Length + encrypted.Length];

		Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
		Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

		return Convert.ToBase64String(result);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="encryptedData"></param>
	/// <returns></returns>
	public static string DecryptAES(string encryptedData)
	{
		try
		{
			byte[] fullCipher = Convert.FromBase64String(encryptedData);

			byte[] iv = new byte[16];
			byte[] cipher = new byte[fullCipher.Length - iv.Length];

			Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
			Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

			using Aes aes = Aes.Create();
			aes.Key = Key;
			aes.IV = iv;

			ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

			using MemoryStream msDecrypt = new(cipher);
			using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
			using StreamReader srDecrypt = new(csDecrypt);

			return srDecrypt.ReadToEnd();
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	private static byte[] GenerateRandomAesKey()
	{
		int[] keyLengths = { 128, 192, 256 };

		int selectedKeyLength = keyLengths[new System.Random().Next(keyLengths.Length)];

		byte[] key = new byte[selectedKeyLength / 8];
		using RNGCryptoServiceProvider rng = new();
		rng.GetBytes(key);

		return key;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="hexString"></param>
	/// <returns></returns>
	public static byte[] ParseHexadecimalString(string hexString)
	{
		hexString = hexString.Replace(" ", "").Replace("\n", "").Replace("\r", "");
		return Enumerable.Range(0, hexString.Length)
						 .Where(x => x % 2 == 0)
						 .Select(x => Convert.ToByte(hexString.Substring(x, 2), 16))
						 .ToArray();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="bytes"></param>
	/// <returns></returns>
	public static string FormatHexadecimalString(byte[] bytes)
	{
		string hexString = BitConverter.ToString(bytes).Replace("-", " ");

		const int columnsPerLine = 16;
		int length = hexString.Length;

		StringBuilder formattedHex = new();
		for (int i = 0; i < length; i += columnsPerLine * 3)
			formattedHex.AppendLine(hexString.Substring(i, Math.Min(columnsPerLine * 3, length - i)).Trim());

		return formattedHex.ToString().Trim();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static string CalculateChecksum(string input)
	{
		using MD5 md5 = MD5.Create();
		byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
		return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static string ApplyROT13(string input)
	{
		string normalAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
		string rot13Alphabet = "nopqrstuvwxyzabcdefghijklmNOPQRSTUVWXYZABCDEFGHIJKLM";

		StringBuilder sb = new();

		for (int i = 0; i < input.Length; i++)
		{
			int index = normalAlphabet.IndexOf(input[i]);

			if (index != -1)
				sb.Append(rot13Alphabet[index]);
			else
				sb.Append(input[i]);
		}
		return sb.ToString();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static string ReverseString(string input) => input.Reverse().ToString();
}
