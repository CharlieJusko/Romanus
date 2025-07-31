using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void EnsureFolder(string path)
    {
        string directoryName = Path.GetDirectoryName(path);
        // If path is a file name only, directory name will be an empty string
        if(directoryName.Length > 0)
        {
            // Create all directories on the path that don't already exist
            Directory.CreateDirectory(directoryName);
        }
    }

    private static void Save<Type>(string path, Type data) where Type : class
    {
        BinaryFormatter formatter = new BinaryFormatter();
        EnsureFolder(path);
        FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    private static Type Load<Type>(string path) where Type : class
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Open);
        Type data = formatter.Deserialize(stream) as Type;
        stream.Close();
        return data;
    }

    #region Level Records
    public static string GetLevelPath(string levelName)
    {
        return Application.persistentDataPath + "/Level Records/" + levelName + ".rec";
    }

    public static void SaveLevel(LevelRecords level)
    {
        string path = GetLevelPath(level.name);
        Save(path, level);
        Debug.Log("Successfully saved level " + level.name);
    }

    public static LevelRecords LoadLevel(string levelName)
    {
        string path = GetLevelPath(levelName);
        if(File.Exists(path))
        {
            LevelRecords records = Load<LevelRecords>(path);
            Debug.Log("Successfully loaded level " + levelName);
            return records;
        } 
        else
        {
            Debug.LogWarning("No save file for level '" + levelName + "' found. Path expected: " + path);
            return null;
        }
    }
    #endregion


    #region Settings
    public static string GetSettingsPath()
    {
        return Application.persistentDataPath + "/Settings.rec";
    }
    public static void SaveSettings(Settings settings)
    {
        string path = GetSettingsPath();
        Save(path, settings);
        Debug.Log("Successfully saved Sattings.");
    }

    public static Settings LoadSettings()
    {
        string path = GetSettingsPath();
        if(File.Exists(path))
        {
            Settings settings = Load<Settings>(path);
            Debug.Log("Successfully loaded settings");
            return settings;
        } 
        else
        {
            Debug.LogWarning("No save file for settings found. Path expected: " + path);
            return null;
        }
    }
    #endregion
}
