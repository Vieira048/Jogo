using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using System.Xml;

public class SaveManager : MonoBehaviour
{
    private const string SaveFileName = "SaveData.json";
    private static string saveDirectoryOverride;

    public static string SaveFilePath => Path.Combine(SaveDirectory, SaveFileName);

    private static string LegacySaveFilePath => Path.Combine(Application.dataPath, SaveFileName);

    private static string SaveDirectory
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(saveDirectoryOverride))
                return saveDirectoryOverride;

            return Application.persistentDataPath;
        }
    }

    public static void OverrideSaveDirectoryForTests(string path)
    {
        saveDirectoryOverride = path;
    }

    public static void ClearSaveDirectoryOverrideForTests()
    {
        saveDirectoryOverride = null;
    }

    //设置游戏数值
    public void SetGameData(Save save)
    {
        GameManager.instance.pesos = save.pesos;
        GameManager.instance.experience = save.experience;
        GameManager.instance.weapon.SetWeaponLevel(save.WeaponLevel);
        GameManager.instance.player.rage = (float)save.rage;    
    }

    //JSON存储
    public void SaveGame()
    {
        //1. 创建存档信息
        Save save = new Save
        {
            pesos = GameManager.instance.pesos,
            experience = GameManager.instance.experience,
            WeaponLevel = GameManager.instance.weapon.weaponLevel,
            rage = (int)GameManager.instance.player.rage
        };

        SaveGame(save);
    }


    public void SaveGame(Save save)
    {
        string path = SaveFilePath;
        string directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        string jsonStr = JsonMapper.ToJson(save);

        File.WriteAllText(path, jsonStr);

        Debug.Log($"Saves: {path}");
    }

    //JSON读取
    public void LoadGame()
    {
        string path = GetReadableSavePath();

        if (File.Exists(path))
        {
            //1. 创建StreamReader用来读取流
            string jsonStr = File.ReadAllText(path);

            //3.
            Save save = JsonMapper.ToObject<Save>(jsonStr);
            SetGameData(save);

            if (path != SaveFilePath)
                SaveGame(save);

            Debug.Log("Game Loaded");
        }
        else
        {
            NewGame();
        }
    }

    //创建新存档
    public void NewGame()
    {
        Save save = new Save
        {
            pesos = 0,
            experience = 0,
            WeaponLevel = 0,
            rage = 0
        };
        SaveGame(save);

        if (GameManager.instance != null)
            SetGameData(save);
    }

    private static string GetReadableSavePath()
    {
        if (File.Exists(SaveFilePath))
            return SaveFilePath;

        if (string.IsNullOrWhiteSpace(saveDirectoryOverride) && File.Exists(LegacySaveFilePath))
            return LegacySaveFilePath;

        return SaveFilePath;
    }
}
