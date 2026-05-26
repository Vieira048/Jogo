using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using NUnit.Framework;
using UnityEngine;

public class SaveManagerRegressionTests
{
    private readonly List<GameObject> objects = new List<GameObject>();
    private readonly List<string> tempDirectories = new List<string>();

    [TearDown]
    public void TearDown()
    {
        SaveManager.ClearSaveDirectoryOverrideForTests();

        for (int i = objects.Count - 1; i >= 0; i--)
        {
            if (objects[i] != null)
                UnityEngine.Object.DestroyImmediate(objects[i]);
        }

        objects.Clear();

        foreach (string directory in tempDirectories)
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }

        tempDirectories.Clear();
    }

    [Test]
    public void SaveFilePathUsesPersistentDataPathInsteadOfApplicationDataPath()
    {
        SaveManager.ClearSaveDirectoryOverrideForTests();

        string savePath = Path.GetFullPath(SaveManager.SaveFilePath);
        string persistentPath = Path.GetFullPath(Application.persistentDataPath);
        string legacyPath = Path.GetFullPath(Path.Combine(Application.dataPath, "SaveData.json"));

        Assert.That(savePath.StartsWith(persistentPath, StringComparison.OrdinalIgnoreCase), Is.True);
        Assert.That(string.Equals(savePath, legacyPath, StringComparison.OrdinalIgnoreCase), Is.False);
    }

    [Test]
    public void SaveGameWritesJsonToWritableSaveDirectory()
    {
        string tempDirectory = CreateTempDirectory();
        SaveManager.OverrideSaveDirectoryForTests(tempDirectory);

        SaveManager manager = CreateObject("SaveManager").AddComponent<SaveManager>();
        Save expected = new Save
        {
            pesos = 17,
            experience = 29,
            WeaponLevel = 3,
            rage = 41
        };

        manager.SaveGame(expected);

        Assert.That(File.Exists(SaveManager.SaveFilePath), Is.True);

        Save actual = JsonMapper.ToObject<Save>(File.ReadAllText(SaveManager.SaveFilePath));
        Assert.That(actual.pesos, Is.EqualTo(expected.pesos));
        Assert.That(actual.experience, Is.EqualTo(expected.experience));
        Assert.That(actual.WeaponLevel, Is.EqualTo(expected.WeaponLevel));
        Assert.That(actual.rage, Is.EqualTo(expected.rage));
    }

    private string CreateTempDirectory()
    {
        string directory = Path.Combine(Path.GetFullPath("Temp"), "SaveManagerRegression", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        tempDirectories.Add(directory);
        return directory;
    }

    private GameObject CreateObject(string name)
    {
        GameObject go = new GameObject(name);
        objects.Add(go);
        return go;
    }
}
