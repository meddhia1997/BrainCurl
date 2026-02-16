using System;
using System.IO;
using UnityEngine;

public sealed class SaveLoadService
{
    private const string FileName = "save.json";

    private string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public bool HasSave()
    {
        try { return File.Exists(FilePath); }
        catch { return false; }
    }

    public bool TryLoad(out GameSaveData data)
    {
        data = null;

        try
        {
            if (!File.Exists(FilePath))
                return false;

            string json = File.ReadAllText(FilePath);
            if (string.IsNullOrWhiteSpace(json))
                return false;

            data = JsonUtility.FromJson<GameSaveData>(json);
            return data != null;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SaveLoad] Load failed: {ex.Message}");
            data = null;
            return false;
        }
    }

    public void Save(GameSaveData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        try
        {
            string json = JsonUtility.ToJson(data, prettyPrint: false);

            // Write via temp file then swap (cross-platform friendly)
            string tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, json);

            if (File.Exists(FilePath))
                File.Delete(FilePath);

            File.Move(tmp, FilePath);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SaveLoad] Save failed: {ex.Message}");
        }
    }

    public void Delete()
    {
        try
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SaveLoad] Delete failed: {ex.Message}");
        }
    }
}
