using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using UnityEngine;

namespace SaveEncryption
{
    /// <summary>
    /// Provides encryption and utility methods for secure data handling.
    /// </summary>
    public static class EncryptionSystem
    {
        /// <summary>
        /// Gets the encryption key used for AES encryption.
        /// </summary>
        public static byte[] Key { get => _key; }

        private static string _path;
        private static readonly byte[] _key;
        private static byte[] _iv;

        private const string SEPARATOR = "|!$*|000";

        /// <summary>
        /// Initializes the <see cref="EncryptionSystem"/> class, loading the encryption key or generating a new one if needed.
        /// </summary>
        static EncryptionSystem()
        {
            _path = Path.Combine(Application.persistentDataPath, "data.sav");
            if (File.Exists(_path))
            {
                string[] fileContents = ReverseString(
                                        ROT13(
                                        Encoding.Default.GetString(
                                        ParseHexadecimalString(
                                        File.ReadAllText(_path))))).Split(SEPARATOR, StringSplitOptions.None);
                if (fileContents.Length == 4 && 
                    fileContents[1] == CalculateChecksum(fileContents[0]) && 
                    fileContents[3] == CalculateChecksum(fileContents[2]))
                {
                    _key = Convert.FromBase64String(fileContents[0]);
                    _iv = Convert.FromBase64String(fileContents[2]);
                }
                else
                {
                    _key = GenerateRandomAesKey();
                    _iv = GenerateRandomAesIV();
                    SaveKeyAndIV();
                }
            }
            else
            {
                _key = GenerateRandomAesKey();
                _iv = GenerateRandomAesIV();
                SaveKeyAndIV();
            }
        }

        private static void SaveKeyAndIV()
        {
            string key = Convert.ToBase64String(_key);
            string iv = Convert.ToBase64String(_iv);
            string data = key + SEPARATOR + CalculateChecksum(key) + SEPARATOR + iv + SEPARATOR + CalculateChecksum(iv);
            File.WriteAllText(_path, FormatHexadecimalString(Encoding.Default.GetBytes(ReverseString(ROT13(data)))));
        }

        /// <summary>
        /// Encrypts the input string using AES encryption.
        /// </summary>
        /// <param name="input">The input string to be encrypted.</param>
        /// <returns>The encrypted string.</returns>
        public static string EncryptAES(string input)
        {
            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

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

                int ivLength = _iv.Length;
                byte[] cipher = new byte[fullCipher.Length - ivLength];
                Array.Copy(fullCipher, ivLength, cipher, 0, cipher.Length);

                using Aes aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;

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
        /// Calculates the SHA-384 checksum of the input string.
        /// </summary>
        /// <param name="input">The input string for which the checksum is calculated.</param>
        /// <returns>The SHA-384 checksum of the input string.</returns>
        public static string CalculateChecksum(string input)
        {
            using SHA384 sha384 = SHA384.Create();
            byte[] hash = sha384.ComputeHash(Encoding.Default.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
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

        private static byte[] GenerateRandomAesIV()
        {
            byte[] iv = new byte[16];
            using RNGCryptoServiceProvider rng = new();
            rng.GetBytes(iv);
            return iv;
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
}
