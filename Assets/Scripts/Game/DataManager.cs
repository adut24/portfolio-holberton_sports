using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using UnityEngine;

/// <summary>
/// Responsible of all data in the game.
/// </summary>
public class DataManager : MonoBehaviour
{
    /// <summary>
    /// Gets or sets the unique identifier associated with the user.
    /// </summary>
    public string UserID { get; private set; }

    /// <summary>
    /// Gets or sets the match history for the user, where each entry is identified by an opponent ID and contains a tuple of two integers representing wins and losses.
    /// </summary>
    public Dictionary<string, (int, int)> MatchHistory { get; set; }

    /// <summary>
    /// Gets or sets the high scores for different sports, where each entry is identified by the sport name and the best score done.
    /// </summary>
    public Dictionary<string, int> HighScores { get; set; }

    private string _filePath;
    private const string SEPARATOR = "|!$*|000";

    /// <summary>
    /// Initializes the DataManager and loads user data if available; otherwise, generates default data.
    /// </summary>
    private void Awake()
    {
        _filePath = Path.Combine(Application.persistentDataPath, "data.sav");
        MatchHistory = new();
        HighScores = new();
        if (File.Exists(_filePath))
        {
            string[] fileContents = EncryptionSystem.ReverseString(
                                    EncryptionSystem.ROT13(
                                    Encoding.Default.GetString(
                                    EncryptionSystem.ParseHexadecimalString(
                                    File.ReadAllText(_filePath))))).Split(SEPARATOR, StringSplitOptions.None);
            if (fileContents.Length == 4
                && fileContents[1] == EncryptionSystem.CalculateChecksum(fileContents[0])
                && fileContents[3] == EncryptionSystem.CalculateChecksum(fileContents[2]))
                LoadData(fileContents[0]);
            else
            {
                InitializeDefaultData();
                SaveData();
            }
        }
        else
        {
            InitializeDefaultData();
            SaveData();
        }
    }

    /// <summary>
    /// Saves the current user data to a file after encrypting and formatting it.
    /// </summary>
    public void SaveData()
    {
        Data data = new() { id = UserID };
        foreach (KeyValuePair<string, int> kvp in HighScores)
            data.highScores.Add(new ScoreEntry { sportName = kvp.Key, score = kvp.Value });
        foreach (KeyValuePair<string, (int, int)> kvp in MatchHistory)
            data.matchHistory.Add(new MatchEntry { opponentID = kvp.Key, results = kvp.Value });

        string encryptedData = EncryptionSystem.EncryptAES(JsonUtility.ToJson(data));
        string dataChecksum = EncryptionSystem.CalculateChecksum(encryptedData);
        string keyAsBase64 = Convert.ToBase64String(EncryptionSystem.Key);
        string keyChecksum = EncryptionSystem.CalculateChecksum(keyAsBase64);
        string combinedData = encryptedData + SEPARATOR + dataChecksum + SEPARATOR + keyAsBase64 + SEPARATOR + keyChecksum;
        string modifiedData = EncryptionSystem.ReverseString(EncryptionSystem.ROT13(combinedData));
        File.WriteAllText(_filePath, EncryptionSystem.FormatHexadecimalString(Encoding.Default.GetBytes(modifiedData)));
    }

    /// <summary>
    /// Loads user data from an encrypted file content, decrypts it, and populates the DataManager's properties with the retrieved data.
    /// </summary>
    /// <param name="fileContent">The encrypted content of the user data file.</param>
    private void LoadData(string fileContent)
    {
        Data data = JsonUtility.FromJson<Data>(EncryptionSystem.DecryptAES(fileContent));
        UserID = data.id;
        foreach (ScoreEntry highScore in data.highScores)
            HighScores.Add(highScore.sportName, highScore.score);
        foreach (MatchEntry opposition in data.matchHistory)
            MatchHistory.Add(opposition.opponentID, opposition.results);
    }

    /// <summary>
    /// Initializes default data, including high scores, a unique user identifier, and populates the DataManager's properties with the default values.
    /// </summary>
    private void InitializeDefaultData()
    {
        HighScores.Add("Bowling", 0);
        HighScores.Add("Archery", 0);
        UserID = Guid.NewGuid().ToString();
    }
}

/// <summary>
/// Represents user-related data, including a unique identifier, high scores, and match history.
/// </summary>
[Serializable]
public class Data
{
    /// <summary>
    /// Represents the unique identifier associated with the user.
    /// </summary>
    public string id;

    /// <summary>
    /// Represents the list of high scores for different sports.
    /// </summary>
    public List<ScoreEntry> highScores;

    /// <summary>
    /// Represents the list of match entries in the user's history.
    /// </summary>
    public List<MatchEntry> matchHistory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Data"/> class.
    /// </summary>
    public Data()
    {
        highScores = new();
        matchHistory = new();
    }
}

/// <summary>
/// Represents a high score entry for a specific sport.
/// </summary>
[Serializable]
public class ScoreEntry
{
    /// <summary>
    /// Represents the name of the sport.
    /// </summary>
    public string sportName;

    /// <summary>
    /// Represents the high score achieved in the sport.
    /// </summary>
    public int score;
}

/// <summary>
/// Represents an entry in the user's match history, including an opponent ID and match results.
/// </summary>
[Serializable]
public class MatchEntry
{
    /// <summary>
    /// Represents the unique identifier of the opponent in the match.
    /// </summary>
    public string opponentID;

    /// <summary>
    /// Represents the tuple of two integers representing match results.
    /// </summary>
    public (int, int) results;
}

/// <summary>
/// Provides encryption and utility methods for secure data handling.
/// </summary>
public static class EncryptionSystem
{
    /// <summary>
    /// Gets the encryption key used for AES encryption.
    /// </summary>
    public static byte[] Key { get => _key; }

    private static readonly byte[] _key;

    /// <summary>
    /// Initializes the <see cref="EncryptionSystem"/> class, loading the encryption key or generating a new one if needed.
    /// </summary>
    static EncryptionSystem()
    {
        string path = Path.Combine(Application.persistentDataPath, "data.sav");
        if (File.Exists(path))
        {
            string[] fileContents = ReverseString(
                                    ROT13(
                                    Encoding.Default.GetString(
                                    ParseHexadecimalString(
                                    File.ReadAllText(path))))).Split("|!$*|000", StringSplitOptions.None);
            if (fileContents.Length == 4 && fileContents[3] == CalculateChecksum(fileContents[2]))
                _key = Convert.FromBase64String(fileContents[2]);
            else
                _key = GenerateRandomAesKey();
        }
        else
            _key = GenerateRandomAesKey();
    }

    /// <summary>
    /// Encrypts the input string using AES encryption.
    /// </summary>
    /// <param name="input">The input string to be encrypted.</param>
    /// <returns>The encrypted string.</returns>
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
    /// Decrypts the encrypted data using AES decryption.
    /// </summary>
    /// <param name="encryptedData">The encrypted data to be decrypted.</param>
    /// <returns>The decrypted string or null if decryption fails.</returns>
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
    /// Generates a random AES key of variable length (128, 192, or 256 bits).
    /// </summary>
    /// <returns>The generated AES key.</returns>
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
    /// Parses a hexadecimal string and returns the corresponding byte array.
    /// </summary>
    /// <param name="hexString">The hexadecimal string to be parsed.</param>
    /// <returns>The byte array represented by the hexadecimal string.</returns>
    public static byte[] ParseHexadecimalString(string hexString)
    {
        hexString = hexString.Replace(" ", "").Replace("\n", "").Replace("\r", "");
        return Enumerable.Range(0, hexString.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hexString.Substring(x, 2), 16))
                         .ToArray();
    }

    /// <summary>
    /// Formats a byte array as a hexadecimal string with optional line breaks for improved readability.
    /// </summary>
    /// <param name="bytes">The byte array to be formatted.</param>
    /// <returns>The formatted hexadecimal string.</returns>
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
    /// Calculates the MD5 checksum of the input string.
    /// </summary>
    /// <param name="input">The input string for which the checksum is calculated.</param>
    /// <returns>The MD5 checksum of the input string.</returns>
    public static string CalculateChecksum(string input)
    {
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Applies the ROT13 substitution cipher to the input string.
    /// </summary>
    /// <param name="input">The string to be transformed using ROT13.</param>
    /// <returns>The ROT13-transformed string.</returns>
    public static string ROT13(string input)
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
    /// Reverses the characters in the input string.
    /// </summary>
    /// <param name="input">The string to be reversed.</param>
    /// <returns>The reversed string.</returns>
    public static string ReverseString(string input) => new(input.Reverse().ToArray());
}
