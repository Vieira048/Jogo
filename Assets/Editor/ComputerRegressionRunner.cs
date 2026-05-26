using System;
using System.Collections.Generic;
using System.IO;
using PinePie.SimpleJoystick;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ComputerRegressionRunner
{
    private static readonly List<string> Passed = new List<string>();
    private static readonly List<GameObject> Objects = new List<GameObject>();

    public static void RunAll()
    {
        int exitCode = 0;
        string resultPath = Path.GetFullPath("TestResults/ComputerRegressionResults.txt");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
            Passed.Clear();

            Run("Player detector does not depend on object name", PlayerDetectorDoesNotDependOnObjectName);
            Run("Portal door uses exit up vector and clamps z", PortalDoorUsesExitUpVectorAndClampsZ);
            Run("Portal door cooldown blocks immediate retrigger", PortalDoorCooldownBlocksImmediateRetrigger);
            Run("Portal runtime colliders are wide triggers", PortalRuntimeCollidersAreWideTriggers);
            Run("Save manager uses writable persistent path", SaveManagerUsesWritablePersistentPath);
            Run("SafeAreaFitter maps safe area to anchors", SafeAreaFitterMapsSafeAreaToAnchors);
            Run("UIManager normalizes responsive mobile layout", UIManagerNormalizesResponsiveMobileLayout);
            Run("Dungeon_0 scene keeps responsive mobile canvas", Dungeon0SceneKeepsResponsiveMobileCanvas);
            Run("Android project settings protect safe area and aspect ratio", AndroidProjectSettingsProtectSafeAreaAndAspectRatio);

            File.WriteAllLines(resultPath, BuildResultLines("PASS", null));
            Debug.Log($"Computer regressions passed. Results: {resultPath}");
        }
        catch (Exception ex)
        {
            exitCode = 1;
            File.WriteAllLines(resultPath, BuildResultLines("FAIL", ex));
            Debug.LogError(ex);
        }
        finally
        {
            CleanupObjects();
            EditorApplication.Exit(exitCode);
        }
    }

    private static void Run(string name, Action test)
    {
        CleanupObjects();
        Portal_Door.ResetTeleportCooldown();
        test();
        Passed.Add(name);
        Debug.Log($"[REGRESSION PASS] {name}");
    }

    private static void PlayerDetectorDoesNotDependOnObjectName()
    {
        GameObject playerRoot = CreateObject("RenamedHero");
        Player player = playerRoot.AddComponent<Player>();

        GameObject childHitbox = CreateObject("BodyCollider");
        childHitbox.transform.SetParent(playerRoot.transform);
        BoxCollider2D collider = childHitbox.AddComponent<BoxCollider2D>();

        AssertTrue(PortalPlayerDetector.TryGetPlayer(collider, out Player detectedPlayer), "Player was not detected.");
        AssertTrue(ReferenceEquals(player, detectedPlayer), "Detected player component is not the expected component.");
    }

    private static void PortalDoorUsesExitUpVectorAndClampsZ()
    {
        Portal_Door portal = CreatePortalDoor(0.5f, 0f, out BoxCollider2D exit);
        exit.transform.position = new Vector3(2f, 3f, 9f);
        exit.transform.rotation = Quaternion.Euler(0f, 0f, -90f);

        GameObject player = CreatePlayer("RenamedHero");
        BoxCollider2D playerCollider = player.AddComponent<BoxCollider2D>();

        AssertTrue(portal.TryTeleport(playerCollider), "Portal door did not teleport the player.");
        AssertVector(player.transform.position, new Vector3(2.5f, 3f, 0f), "Portal door destination is wrong.");
    }

    private static void PortalDoorCooldownBlocksImmediateRetrigger()
    {
        Portal_Door portal = CreatePortalDoor(0.5f, 10f, out BoxCollider2D exit);
        exit.transform.position = new Vector3(1f, 1f, 0f);

        GameObject player = CreatePlayer("Player");
        BoxCollider2D playerCollider = player.AddComponent<BoxCollider2D>();

        AssertTrue(portal.TryTeleport(playerCollider), "First teleport should succeed.");

        player.transform.position = Vector3.zero;
        AssertTrue(!portal.TryTeleport(playerCollider), "Second teleport should be blocked by cooldown.");
        AssertVector(player.transform.position, Vector3.zero, "Cooldown allowed player movement.");
    }

    private static void PortalRuntimeCollidersAreWideTriggers()
    {
        Portal_Door door = CreatePortalDoor(0.5f, 0f, out _);
        BoxCollider2D doorCollider = door.GetComponent<BoxCollider2D>();
        doorCollider.size = new Vector2(0.1f, 0.1f);
        door.ConfigureRuntimeCollider();

        AssertTrue(doorCollider.isTrigger, "Room portal collider must be trigger.");
        AssertTrue(doorCollider.size.x >= door.minimumTriggerSize.x, "Room portal collider is too narrow.");
        AssertTrue(doorCollider.size.y >= door.minimumTriggerSize.y, "Room portal collider is too short.");

        GameObject scenePortalObject = CreateObject("ScenePortal");
        BoxCollider2D sceneCollider = scenePortalObject.AddComponent<BoxCollider2D>();
        sceneCollider.size = new Vector2(0.1f, 0.1f);
        Portal scenePortal = scenePortalObject.AddComponent<Portal>();
        scenePortal.ConfigureRuntimeCollider();

        AssertTrue(sceneCollider.isTrigger, "Scene portal collider must be trigger.");
        AssertTrue(sceneCollider.size.x >= scenePortal.minimumTriggerSize.x, "Scene portal collider is too narrow.");
        AssertTrue(sceneCollider.size.y >= scenePortal.minimumTriggerSize.y, "Scene portal collider is too short.");
    }

    private static void SaveManagerUsesWritablePersistentPath()
    {
        SaveManager.ClearSaveDirectoryOverrideForTests();

        string savePath = Path.GetFullPath(SaveManager.SaveFilePath);
        string persistentPath = Path.GetFullPath(Application.persistentDataPath);
        string legacyPath = Path.GetFullPath(Path.Combine(Application.dataPath, "SaveData.json"));

        AssertTrue(savePath.StartsWith(persistentPath, StringComparison.OrdinalIgnoreCase), "Save path must use Application.persistentDataPath.");
        AssertTrue(!string.Equals(savePath, legacyPath, StringComparison.OrdinalIgnoreCase), "Save path must not point inside Application.dataPath.");

        string tempDirectory = Path.Combine(Path.GetFullPath("Temp"), "SaveManagerRegression", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        SaveManager.OverrideSaveDirectoryForTests(tempDirectory);

        try
        {
            SaveManager manager = CreateObject("SaveManager").AddComponent<SaveManager>();
            manager.SaveGame(new Save
            {
                pesos = 17,
                experience = 29,
                WeaponLevel = 3,
                rage = 41
            });

            AssertTrue(File.Exists(SaveManager.SaveFilePath), "SaveManager did not write a save file.");

            string json = File.ReadAllText(SaveManager.SaveFilePath);
            AssertTrue(json.Contains("\"pesos\":17"), "Save file did not contain pesos.");
            AssertTrue(json.Contains("\"experience\":29"), "Save file did not contain experience.");
            AssertTrue(json.Contains("\"WeaponLevel\":3"), "Save file did not contain weapon level.");
            AssertTrue(json.Contains("\"rage\":41"), "Save file did not contain rage.");
        }
        finally
        {
            SaveManager.ClearSaveDirectoryOverrideForTests();

            if (Directory.Exists(tempDirectory))
                Directory.Delete(tempDirectory, true);
        }
    }

    private static void SafeAreaFitterMapsSafeAreaToAnchors()
    {
        GameObject go = CreateObject("SafeArea");
        RectTransform rectTransform = go.AddComponent<RectTransform>();
        SafeAreaFitter fitter = go.AddComponent<SafeAreaFitter>();

        fitter.ApplySafeArea(new Rect(120f, 40f, 2160f, 1000f), new Vector2(2400f, 1080f));

        AssertVector(rectTransform.anchorMin, new Vector2(0.05f, 0.03703704f), "Safe area anchorMin is wrong.");
        AssertVector(rectTransform.anchorMax, new Vector2(0.95f, 0.962963f), "Safe area anchorMax is wrong.");
        AssertVector(rectTransform.anchoredPosition, Vector2.zero, "Safe area anchoredPosition must be zero.");
        AssertVector(rectTransform.sizeDelta, Vector2.zero, "Safe area sizeDelta must be zero.");
    }

    private static void UIManagerNormalizesResponsiveMobileLayout()
    {
        GameObject root = CreateObject("UIManager");
        root.AddComponent<RectTransform>();
        CanvasScaler rootScaler = root.AddComponent<CanvasScaler>();
        UIManager manager = root.AddComponent<UIManager>();

        GameObject hud = CreateObject("HUD");
        hud.transform.SetParent(root.transform);
        hud.AddComponent<RectTransform>();
        manager.characterHUD = hud.AddComponent<CharacterHUD>();

        GameObject controls = CreateObject("ControlesMobile");
        controls.transform.SetParent(hud.transform);
        RectTransform controlsRect = controls.AddComponent<RectTransform>();
        CanvasScaler controlsScaler = controls.AddComponent<CanvasScaler>();
        controlsRect.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        controlsRect.anchorMin = Vector2.zero;
        controlsRect.anchorMax = Vector2.zero;
        controlsRect.anchoredPosition = new Vector2(300f, 200f);
        controlsRect.sizeDelta = new Vector2(907f, 657f);
        manager.controlesMobile = controls;

        MobileInputManager mobileInput = controls.AddComponent<MobileInputManager>();

        GameObject joystick = CreateObject("PinePie Joystick");
        joystick.transform.SetParent(controls.transform);
        RectTransform joystickRect = joystick.AddComponent<RectTransform>();
        mobileInput.joystick = joystick.AddComponent<JoystickController>();

        GameObject attack = CreateObject("AttackButton");
        attack.transform.SetParent(controls.transform);
        RectTransform attackRect = attack.AddComponent<RectTransform>();
        mobileInput.attackButton = attack.AddComponent<Button>();

        GameObject rage = CreateObject("RageButton");
        rage.transform.SetParent(controls.transform);
        RectTransform rageRect = rage.AddComponent<RectTransform>();
        mobileInput.rageButton = rage.AddComponent<Button>();

        GameObject menu = CreateObject("MenuButton");
        menu.transform.SetParent(hud.transform);
        RectTransform menuRect = menu.AddComponent<RectTransform>();

        manager.ConfigureResponsiveLayout();

        AssertScaler(rootScaler);
        AssertScaler(controlsScaler);
        AssertTrue(hud.GetComponent<SafeAreaFitter>() != null, "HUD needs SafeAreaFitter.");
        AssertTrue(controls.GetComponent<SafeAreaFitter>() != null, "Mobile controls need SafeAreaFitter.");
        AssertVector(controlsRect.anchorMin, Vector2.zero, "Controls anchorMin must stretch.");
        AssertVector(controlsRect.anchorMax, Vector2.one, "Controls anchorMax must stretch.");
        AssertVector(controlsRect.anchoredPosition, Vector2.zero, "Controls anchoredPosition must reset.");
        AssertVector(controlsRect.sizeDelta, Vector2.zero, "Controls sizeDelta must reset.");
        AssertVector(controlsRect.localScale, Vector3.one, "Controls scale must reset.");
        AssertAnchoredControl(joystickRect, Vector2.zero, Vector2.zero, manager.joystickMargin, manager.joystickSize, "Joystick must be bottom-left.");
        AssertAnchoredControl(attackRect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-manager.attackButtonMargin.x, manager.attackButtonMargin.y), manager.attackButtonSize, "Attack button must be bottom-right.");
        AssertAnchoredControl(rageRect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-manager.rageButtonMargin.x, manager.rageButtonMargin.y), manager.rageButtonSize, "Rage button must be bottom-right.");
        AssertAnchoredControl(menuRect, Vector2.one, Vector2.one, new Vector2(-manager.menuButtonMargin.x, -manager.menuButtonMargin.y), manager.menuButtonSize, "Menu chest must be top-right.");
    }

    private static void Dungeon0SceneKeepsResponsiveMobileCanvas()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/_Scenes/Dungeon_0.unity", OpenSceneMode.Single);
        AssertTrue(scene.IsValid(), "Dungeon_0 scene could not be opened.");

        GameObject controls = GameObject.Find("ControlesMobile");
        AssertTrue(controls != null, "Dungeon_0 must contain ControlesMobile.");
        AssertScaler(controls.GetComponent<CanvasScaler>());

        RectTransform controlsRect = controls.GetComponent<RectTransform>();
        AssertVector(controlsRect.anchorMin, Vector2.zero, "Scene controls anchorMin must stretch.");
        AssertVector(controlsRect.anchorMax, Vector2.one, "Scene controls anchorMax must stretch.");
        AssertVector(controlsRect.anchoredPosition, Vector2.zero, "Scene controls anchoredPosition must reset.");
        AssertVector(controlsRect.sizeDelta, Vector2.zero, "Scene controls sizeDelta must reset.");
        AssertVector(controlsRect.localScale, Vector3.one, "Scene controls scale must reset.");

        GameObject uiManagerObject = GameObject.Find("UIManager");
        AssertTrue(uiManagerObject != null, "Dungeon_0 must contain UIManager.");
        AssertScaler(uiManagerObject.GetComponent<CanvasScaler>());

        UIManager uiManager = uiManagerObject.GetComponent<UIManager>();
        AssertTrue(uiManager != null, "UIManager component is missing.");
        AssertVector(uiManager.mobileReferenceResolution, new Vector2(1280f, 720f), "UIManager reference resolution is wrong.");
        AssertFloat(uiManager.mobileMatchWidthOrHeight, 0.5f, "UIManager match value is wrong.");

        uiManager.ConfigureResponsiveLayout();

        MobileInputManager mobileInput = controls.GetComponentInChildren<MobileInputManager>(true);
        AssertTrue(mobileInput != null, "Dungeon_0 must contain MobileInputManager.");
        AssertTrue(mobileInput.joystick != null, "Dungeon_0 must wire joystick reference.");
        AssertAnchoredControl(mobileInput.joystick.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero, uiManager.joystickMargin, uiManager.joystickSize, "Scene joystick must be bottom-left.");

        GameObject menuButton = GameObject.Find("MenuButton");
        AssertTrue(menuButton != null, "Dungeon_0 must contain MenuButton.");
        AssertAnchoredControl(menuButton.GetComponent<RectTransform>(), Vector2.one, Vector2.one, new Vector2(-uiManager.menuButtonMargin.x, -uiManager.menuButtonMargin.y), uiManager.menuButtonSize, "Scene menu chest must be top-right.");
    }

    private static void AndroidProjectSettingsProtectSafeAreaAndAspectRatio()
    {
        string settings = File.ReadAllText("ProjectSettings/ProjectSettings.asset");
        AssertTrue(settings.Contains("androidRenderOutsideSafeArea: 0"), "Android must not render UI outside safe area.");
        AssertTrue(settings.Contains("androidMaxAspectRatio: 2.4"), "Android max aspect ratio must support modern widescreen phones.");
    }

    private static Portal_Door CreatePortalDoor(float exitOffset, float cooldown, out BoxCollider2D exit)
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

    private static GameObject CreatePlayer(string name)
    {
        GameObject player = CreateObject(name);
        player.AddComponent<Player>();
        return player;
    }

    private static GameObject CreateObject(string name)
    {
        GameObject go = new GameObject(name);
        Objects.Add(go);
        return go;
    }

    private static void CleanupObjects()
    {
        for (int i = Objects.Count - 1; i >= 0; i--)
        {
            if (Objects[i] != null)
                UnityEngine.Object.DestroyImmediate(Objects[i]);
        }

        Objects.Clear();
        Portal_Door.ResetTeleportCooldown();
        SaveManager.ClearSaveDirectoryOverrideForTests();
    }

    private static void AssertScaler(CanvasScaler scaler)
    {
        AssertTrue(scaler != null, "CanvasScaler is missing.");
        AssertTrue(scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize, "CanvasScaler must scale with screen size.");
        AssertVector(scaler.referenceResolution, new Vector2(1280f, 720f), "CanvasScaler reference resolution is wrong.");
        AssertTrue(scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight, "CanvasScaler match mode is wrong.");
        AssertFloat(scaler.matchWidthOrHeight, 0.5f, "CanvasScaler match value is wrong.");
    }

    private static void AssertAnchoredControl(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, string message)
    {
        AssertTrue(rect != null, message);
        AssertVector(rect.anchorMin, anchor, $"{message} anchorMin");
        AssertVector(rect.anchorMax, anchor, $"{message} anchorMax");
        AssertVector(rect.pivot, pivot, $"{message} pivot");
        AssertVector(rect.anchoredPosition, position, $"{message} position");
        AssertVector(rect.sizeDelta, size, $"{message} size");
        AssertVector(rect.localScale, Vector3.one, $"{message} scale");
    }

    private static void AssertTrue(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private static void AssertFloat(float actual, float expected, string message)
    {
        if (Mathf.Abs(actual - expected) > 0.0001f)
            throw new InvalidOperationException($"{message} Expected {expected}, got {actual}.");
    }

    private static void AssertVector(Vector2 actual, Vector2 expected, string message)
    {
        if (Vector2.Distance(actual, expected) > 0.0001f)
            throw new InvalidOperationException($"{message} Expected {expected}, got {actual}.");
    }

    private static void AssertVector(Vector3 actual, Vector3 expected, string message)
    {
        if (Vector3.Distance(actual, expected) > 0.0001f)
            throw new InvalidOperationException($"{message} Expected {expected}, got {actual}.");
    }

    private static IEnumerable<string> BuildResultLines(string status, Exception exception)
    {
        yield return $"status={status}";
        yield return $"passed={Passed.Count}";

        foreach (string passed in Passed)
            yield return $"PASS {passed}";

        if (exception != null)
        {
            yield return "ERROR";
            yield return exception.ToString();
        }
    }
}
