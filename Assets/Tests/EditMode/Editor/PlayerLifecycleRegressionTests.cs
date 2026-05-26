using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlayerLifecycleRegressionTests
{
    private readonly List<GameObject> objects = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        for (int i = objects.Count - 1; i >= 0; i--)
        {
            if (objects[i] != null)
                Object.DestroyImmediate(objects[i]);
        }

        objects.Clear();
    }

    [Test]
    public void ResetForGameplayRestoresMovementStateAfterDeath()
    {
        GameObject playerObject = CreateObject("Player");
        BoxCollider2D collider = playerObject.AddComponent<BoxCollider2D>();
        Player player = playerObject.AddComponent<Player>();

        player.maxHitPoint = 25;
        player.hitPoint = 0;
        player.isAlive = false;
        player.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
        collider.enabled = false;
        playerObject.SetActive(false);

        player.ResetForGameplay(true, true);

        Assert.IsTrue(playerObject.activeSelf);
        Assert.IsTrue(player.enabled);
        Assert.IsTrue(player.isAlive);
        Assert.AreEqual(player.maxHitPoint, player.hitPoint);
        Assert.IsTrue(collider.enabled);
        AssertVector(player.transform.localEulerAngles, Vector3.zero);
        Assert.AreEqual(0f, player.rage);
    }

    [Test]
    public void ResetForGameplayCanPreserveCurrentHealthOnSceneTransition()
    {
        GameObject playerObject = CreateObject("Player");
        BoxCollider2D collider = playerObject.AddComponent<BoxCollider2D>();
        Player player = playerObject.AddComponent<Player>();

        player.maxHitPoint = 30;
        player.hitPoint = 12;
        player.isAlive = true;
        player.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
        collider.enabled = false;

        player.ResetForGameplay(false);

        Assert.IsTrue(player.isAlive);
        Assert.AreEqual(12, player.hitPoint);
        Assert.IsTrue(collider.enabled);
        AssertVector(player.transform.localEulerAngles, Vector3.zero);
    }

    private GameObject CreateObject(string name)
    {
        GameObject go = new GameObject(name);
        objects.Add(go);
        return go;
    }

    private static void AssertVector(Vector3 actual, Vector3 expected)
    {
        Assert.That(Vector3.Distance(actual, expected), Is.LessThan(0.0001f), $"Expected {expected}, got {actual}");
    }
}
