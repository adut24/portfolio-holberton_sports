using Models;

#if UNITY_STANDALONE_WIN
using Mono.Data.Sqlite;
#endif
using SaveEncryption;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

using UnityEngine;

public class DataManager : MonoBehaviour
{
    public User CurrentUser { get; private set; }
#if UNITY_STANDALONE_WIN
    private string _connectionString;
    private IDbConnection _dbConnection;
#elif UNITY_ANDROID
    /// <summary>
    /// Gets or sets the match history for the user, where each entry is identified by an opponent ID and contains a tuple of two integers representing wins and losses.
    /// </summary>
    public Dictionary<string, (string, int, int)> MatchHistory { get; set; }

    /// <summary>
    /// Gets or sets the high scores for different sports, where each entry is identified by the sport name and the best score done.
    /// </summary>
    public Dictionary<string, int> HighScores { get; set; }

    private string _filePath;
    private const string SEPARATOR = "|!$*|000";
#endif

    private void Awake()
    {
#if UNITY_STANDALONE_WIN
        _connectionString = $"URI=file:{Path.Combine(Application.persistentDataPath, "data.db")}";
        _dbConnection = new SqliteConnection(_connectionString);
        _dbConnection.Open();
        InitializeDatabase();
        if (IsUsersTableEmpty())
        {
            CurrentUser = new();
            InsertUser(CurrentUser);
        }
        else
            RetrieveUser();
#elif UNITY_ANDROID
        _filePath = Path.Combine(Application.persistentDataPath, "user.sav");
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
#endif
    }


#if UNITY_STANDALONE_WIN
    private void OnDestroy() => _dbConnection.Close();

    private void InitializeDatabase()
    {
        CreateUserTable();
        CreateHighScoreTable();
        CreateMatchHistoryTable();
        CreateVolumeSettingsTable();
        CreateAccessibilitySettingsTable();
    }

    public IDataReader ExecuteQuery(ReadOnlySpan<char> query, Dictionary<string, object> parameters, bool isGettingData = false)
    {
        try
        {
            using IDbCommand dbCommand = _dbConnection.CreateCommand();
            dbCommand.CommandText = query.ToString();

            foreach (KeyValuePair<string, object> parameter in parameters)
            {
                IDbDataParameter queryParameters = dbCommand.CreateParameter();
                queryParameters.ParameterName = parameter.Key;
                queryParameters.Value = parameter.Value;
                dbCommand.Parameters.Add(queryParameters);
            }

            if (!isGettingData)
            {
                dbCommand.ExecuteNonQuery();
                return null;
            }
            else
                return dbCommand.ExecuteReader();
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
            return null;
        }
    }

    private void CreateUserTable()
    {
        const string query =
        @"CREATE TABLE IF NOT EXISTS users (
          id TEXT NOT NULL PRIMARY KEY,
          nickname TEXT,
          checksum TEXT NOT NULL);";
        ExecuteQuery(query, new());
    }

    private void CreateHighScoreTable()
    {
        const string query =
        @"CREATE TABLE IF NOT EXISTS high_scores (
          id TEXT NOT NULL PRIMARY KEY,
          user_id TEXT NOT NULL,
          sport TEXT NOT NULL,
          score TEXT,
          checksum TEXT NOT NULL,
          FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE);";
        ExecuteQuery(query, new());
    }

    private void CreateMatchHistoryTable()
    {
        const string query =
        @"CREATE TABLE IF NOT EXISTS match_history (
          id TEXT NOT NULL PRIMARY KEY,
          user_id TEXT NOT NULL,
          opponent_id TEXT NOT NULL,
          sport TEXT NOT NULL,
          victories TEXT,
          defeats TEXT,
          checksum TEXT NOT NULL,
          FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE);";
        ExecuteQuery(query, new());
    }

    private void CreateVolumeSettingsTable()
    {
        const string query =
        @"CREATE TABLE IF NOT EXISTS volume_settings (
          id TEXT NOT NULL PRIMARY KEY,
          user_id TEXT NOT NULL,
          bgm REAL CHECK (bgm >= 0 AND bgm <= 1),
          sfx REAL CHECK (sfx >= 0 AND sfx <= 1),
          FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE);";
        ExecuteQuery(query, new());
    }

    private void CreateAccessibilitySettingsTable()
    {
        const string query =
        @"CREATE TABLE IF NOT EXISTS accessibility_settings (
          id TEXT NOT NULL PRIMARY KEY,
          user_id TEXT NOT NULL,
          reduced_mobility BOOLEAN,
          one_handed BOOLEAN,
          FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE);";
        ExecuteQuery(query, new());
    }

    private void InsertUser(User user)
    {
        const string query =
        @"INSERT INTO users (id, nickname, checksum) 
          VALUES ($id, $nickname, $checksum);";
        string encryptedId = EncryptionSystem.EncryptAES(user.id);
        string encryptedNickname = EncryptionSystem.EncryptAES(user.nickname);
        Dictionary<string, object> parameters = new()
        {
            { "$id", encryptedId },
            { "$nickname", encryptedNickname },
            { "$checksum", EncryptionSystem.CalculateChecksum($"{encryptedId}{encryptedNickname}") }
        };
        ExecuteQuery(query, parameters);
    }

    private void RetrieveUser()
    {
        const string query =
        @"SELECT id, nickname, checksum 
          FROM users 
          LIMIT 1;";
        using IDataReader reader = ExecuteQuery(query, new(), true);
        if (reader.Read())
        {
            string encryptedId = reader.GetString(0);
            string encryptedNickname = reader[1] == DBNull.Value ? null : reader.GetString(1);
            string storedChecksum = reader.GetString(2);
            if (EncryptionSystem.CalculateChecksum($"{encryptedId}{encryptedNickname}") != storedChecksum)
                CurrentUser = new User();
            else
                CurrentUser = new User(EncryptionSystem.DecryptAES(encryptedId), EncryptionSystem.DecryptAES(encryptedNickname));
        }
    }

    private bool IsUsersTableEmpty()
    {
        const string query =
        @"SELECT COUNT(*) 
          FROM users;";
        using IDataReader reader = ExecuteQuery(query, new(), true);
        return !reader.Read() || reader.GetInt32(0) == 0;
    }
#elif UNITY_ANDROID
    /// <summary>
    /// Saves the current user data to a file after encrypting and formatting it.
    /// </summary>
    public void SaveData()
    {
        User user = new(CurrentUser.id);
        foreach (KeyValuePair<string, int> kvp in HighScores)
            user.highScores.Add(new HighScore(null, CurrentUser.id, kvp.Key, kvp.Value));
        foreach (KeyValuePair<string, (string, int, int)> kvp in MatchHistory)
        {
            (string, int, int) values = kvp.Value;
            user.matchHistory.Add(new MatchHistory(null, CurrentUser.id, kvp.Key, values.Item1, values.Item2, values.Item3));
        }

        string encryptedData = EncryptionSystem.EncryptAES(JsonUtility.ToJson(user));
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
        User user = JsonUtility.FromJson<User>(EncryptionSystem.DecryptAES(fileContent));
        foreach (HighScore highScore in user.highScores)
            HighScores.Add(highScore.sport, highScore.score);
        foreach (MatchHistory match in user.matchHistory)
            MatchHistory.Add(match.opponentID, (match.sport, match.victories, match.defeats));
        CurrentUser = user;
    }

    /// <summary>
    /// Initializes default data, including high scores, a unique user identifier, and populates the DataManager's properties with the default values.
    /// </summary>
    private void InitializeDefaultData()
    {
        HighScores.Add("Bowling", 0);
        HighScores.Add("Archery", 0);
        CurrentUser = new();
	}
#endif
}
