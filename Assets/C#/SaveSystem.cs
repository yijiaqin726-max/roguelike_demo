using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private const string SaveFileName = "demo_save.json";

    private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public static void Save(GameSaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Saved run data to: " + SavePath);
    }

    public static GameSaveData Load()
    {
        if (!HasSave())
        {
            return null;
        }

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<GameSaveData>(json);
    }

    public static void DeleteSave()
    {
        if (HasSave())
        {
            File.Delete(SavePath);
        }
    }
}
