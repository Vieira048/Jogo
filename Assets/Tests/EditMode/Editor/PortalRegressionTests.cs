using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PortalRegressionTests
{
    private readonly List<GameObject> objects = new List<GameObject>();

    [SetUp]
    public void SetUp()
    {
        Portal_Door.ResetTeleportCooldown();
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = objects.Count - 1; i >= 0; i--)
        {
            if (objects[i] != null)
                Object.DestroyImmediate(objects[i]);
        }

        objects.Clear();
        Portal_Door.ResetTeleportCooldown();
    }

    [Test]
    public void PlayerDetectorFindsPlayerByComponentNotObjectName()
    {
        GameObject playerRoot = CreateObject("RenamedHero");
        Player player = playerRoot.AddComponent<Player>();

        GameObject childHitbox = CreateObject("BodyCollider");
        childHitbox.transform.SetParent(playerRoot.transform);
        BoxCollider2D collider = childHitbox.AddComponent<BoxCollider2D>();

        bool detected = PortalPlayerDetector.TryGetPlayer(collider, out Player detectedPlayer);

        Assert.IsTrue(detected);
        Assert.AreSame(player, detectedPlayer);
    }

    [Test]
    public void PortalDoorTeleportsAlongExitUpVectorAndClampsZToZero()
    {
        Portal_Door portal = CreatePortalDoor(0.5f, 0f, out BoxCollider2D exit);
        exit.transform.position = new Vector3(2f, 3f, 9f);
        exit.transform.rotation = Quaternion.Euler(0f, 0f, -90f);

        BoxCollider2D playerCollider = CreatePlayer("RenamedHero").AddComponent<BoxCollider2D>();

        bool teleported = portal.TryTeleport(playerCollider);

        Assert.IsTrue(teleported);
        AssertVector(playerCollider.transform.position, new Vector3(2.5f, 3f, 0f));
    }

    [Test]
    public void PortalDoorIgnoresObjectsWithoutPlayerComponent()
    {
        Portal_Door portal = CreatePortalDoor(0.5f, 0f, out _);
        GameObject crate = CreateObject("Crate");
        BoxCollider2D crateCollider = crate.AddComponent<BoxCollider2D>();

        bool teleported = portal.TryTeleport(crateCollider);

        Assert.IsFalse(teleported);
    }

    [Test]
    public void PortalDoorCooldownBlocksImmediateRetrigger()
    {
        Portal_Door portal = CreatePortalDoor(0.5f, 10f, out BoxCollider2D exit);
        exit.transform.position = new Vector3(1f, 1f, 0f);

        GameObject player = CreatePlayer("Player");
        BoxCollider2D playerCollider = player.AddComponent<BoxCollider2D>();

        Assert.IsTrue(portal.TryTeleport(playerCollider));

        player.transform.position = Vector3.zero;
        Assert.IsFalse(portal.TryTeleport(playerCollider));
        AssertVector(player.transform.position, Vector3.zero);
    }

    [Test]
    public void PortalDoorRuntimeColliderIsTriggerAndHasMinimumTouchArea()
    {
        Portal_Door portal = CreatePortalDoor(0.5f, 0f, out _);
        BoxCollider2D collider = portal.GetComponent<BoxCollider2D>();
        collider.size = new Vector2(0.1f, 0.1f);

        portal.ConfigureRuntimeCollider();

        Assert.IsTrue(collider.isTrigger);
        Assert.That(collider.size.x, Is.GreaterThanOrEqualTo(portal.minimumTriggerSize.x));
        Assert.That(collider.size.y, Is.GreaterThanOrEqualTo(portal.minimumTriggerSize.y));
    }

    [Test]
    public void ScenePortalRuntimeColliderIsTriggerAndHasMinimumTouchArea()
    {
        GameObject portalObject = CreateObject("ScenePortal");
        BoxCollider2D collider = portalObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.1f, 0.1f);
        Portal portal = portalObject.AddComponent<Portal>();

        portal.ConfigureRuntimeCollider();

        Assert.IsTrue(collider.isTrigger);
        Assert.That(collider.size.x, Is.GreaterThanOrEqualTo(portal.minimumTriggerSize.x));
        Assert.That(collider.size.y, Is.GreaterThanOrEqualTo(portal.minimumTriggerSize.y));
    }

    private Portal_Door CreatePortalDoor(float exitOffset, float cooldown, out BoxCollider2D exit)
    {
        GameObject portalObject = CreateObject("PortalDoor");
        portalObject.AddComponent<BoxCollider2D>();
        Portal_Door portal = portalObject.AddComponent<Portal_Door>();
        portal.exitOffset = exitOffset;
        portal.teleportCooldown = cooldown;

        GameObject exitObject = CreateObject("Exit");
        exit = exitObject.AddComponent<BoxCollider2D>();
        portal.boxOUT = exit;

        return portal;
    }

    private GameObject CreatePlayer(string name)
    {
        GameObject player = CreateObject(name);
        player.AddComponent<Player>();
        return player;
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
