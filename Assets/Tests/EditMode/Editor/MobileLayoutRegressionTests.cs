using System.Collections.Generic;
using NUnit.Framework;
using PinePie.SimpleJoystick;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MobileLayoutRegressionTests
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
    public void SafeAreaFitterConvertsSafeAreaToAnchors()
    {
        GameObject go = CreateObject("SafeArea");
        RectTransform rectTransform = go.AddComponent<RectTransform>();
        SafeAreaFitter fitter = go.AddComponent<SafeAreaFitter>();

        fitter.ApplySafeArea(new Rect(120f, 40f, 2160f, 1000f), new Vector2(2400f, 1080f));

        AssertVector(rectTransform.anchorMin, new Vector2(0.05f, 0.03703704f));
        AssertVector(rectTransform.anchorMax, new Vector2(0.95f, 0.962963f));
        AssertVector(rectTransform.anchoredPosition, Vector2.zero);
        AssertVector(rectTransform.sizeDelta, Vector2.zero);
    }

    [Test]
    public void UIManagerConfiguresRuntimeMobileLayout()
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

        GameObject joystickObject = CreateObject("PinePie Joystick");
        joystickObject.transform.SetParent(controls.transform);
        RectTransform joystickRect = joystickObject.AddComponent<RectTransform>();
        mobileInput.joystick = joystickObject.AddComponent<JoystickController>();

        GameObject attackObject = CreateObject("AttackButton");
        attackObject.transform.SetParent(controls.transform);
        RectTransform attackRect = attackObject.AddComponent<RectTransform>();
        mobileInput.attackButton = attackObject.AddComponent<Button>();

        GameObject rageObject = CreateObject("RageButton");
        rageObject.transform.SetParent(controls.transform);
        RectTransform rageRect = rageObject.AddComponent<RectTransform>();
        mobileInput.rageButton = rageObject.AddComponent<Button>();

        GameObject menuObject = CreateObject("MenuButton");
        menuObject.transform.SetParent(hud.transform);
        RectTransform menuRect = menuObject.AddComponent<RectTransform>();

        manager.ConfigureResponsiveLayout();

        AssertScaler(rootScaler);
        AssertScaler(controlsScaler);
        Assert.IsNotNull(hud.GetComponent<SafeAreaFitter>());
        Assert.IsNotNull(controls.GetComponent<SafeAreaFitter>());
        AssertVector(controlsRect.anchorMin, Vector2.zero);
        AssertVector(controlsRect.anchorMax, Vector2.one);
        AssertVector(controlsRect.anchoredPosition, Vector2.zero);
        AssertVector(controlsRect.sizeDelta, Vector2.zero);
        AssertVector(controlsRect.localScale, Vector3.one);

        AssertCorner(joystickRect, Vector2.zero, Vector2.zero, manager.joystickMargin, manager.joystickSize);
        AssertCorner(attackRect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-manager.attackButtonMargin.x, manager.attackButtonMargin.y), manager.attackButtonSize);
        AssertCorner(rageRect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-manager.rageButtonMargin.x, manager.rageButtonMargin.y), manager.rageButtonSize);
        AssertCorner(menuRect, Vector2.one, Vector2.one, new Vector2(-manager.menuButtonMargin.x, -manager.menuButtonMargin.y), manager.menuButtonSize);
    }

    [Test]
    public void Dungeon0SceneKeepsMobileCanvasResponsive()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/_Scenes/Dungeon_0.unity", OpenSceneMode.Single);
        Assert.IsTrue(scene.IsValid());

        GameObject controls = GameObject.Find("ControlesMobile");
        Assert.IsNotNull(controls, "Dungeon_0 must contain ControlesMobile.");
        CanvasScaler controlsScaler = controls.GetComponent<CanvasScaler>();
        RectTransform controlsRect = controls.GetComponent<RectTransform>();

        AssertScaler(controlsScaler);
        AssertVector(controlsRect.anchorMin, Vector2.zero);
        AssertVector(controlsRect.anchorMax, Vector2.one);
        AssertVector(controlsRect.anchoredPosition, Vector2.zero);
        AssertVector(controlsRect.sizeDelta, Vector2.zero);
        AssertVector(controlsRect.localScale, Vector3.one);

        GameObject uiManagerObject = GameObject.Find("UIManager");
        Assert.IsNotNull(uiManagerObject, "Dungeon_0 must contain UIManager.");
        AssertScaler(uiManagerObject.GetComponent<CanvasScaler>());

        UIManager uiManager = uiManagerObject.GetComponent<UIManager>();
        Assert.IsNotNull(uiManager);
        AssertVector(uiManager.mobileReferenceResolution, new Vector2(1280f, 720f));
        Assert.That(uiManager.mobileMatchWidthOrHeight, Is.EqualTo(0.5f).Within(0.0001f));

        uiManager.ConfigureResponsiveLayout();

        MobileInputManager mobileInput = controls.GetComponentInChildren<MobileInputManager>(true);
        Assert.IsNotNull(mobileInput, "Dungeon_0 must contain MobileInputManager.");
        Assert.IsNotNull(mobileInput.joystick, "Dungeon_0 must wire the joystick reference.");
        AssertCorner(mobileInput.joystick.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero, uiManager.joystickMargin, uiManager.joystickSize);

        RectTransform menuButton = GameObject.Find("MenuButton").GetComponent<RectTransform>();
        AssertCorner(menuButton, Vector2.one, Vector2.one, new Vector2(-uiManager.menuButtonMargin.x, -uiManager.menuButtonMargin.y), uiManager.menuButtonSize);
    }

    private GameObject CreateObject(string name)
    {
        GameObject go = new GameObject(name);
        objects.Add(go);
        return go;
    }

    private static void AssertScaler(CanvasScaler scaler)
    {
        Assert.IsNotNull(scaler);
        Assert.AreEqual(CanvasScaler.ScaleMode.ScaleWithScreenSize, scaler.uiScaleMode);
        AssertVector(scaler.referenceResolution, new Vector2(1280f, 720f));
        Assert.AreEqual(CanvasScaler.ScreenMatchMode.MatchWidthOrHeight, scaler.screenMatchMode);
        Assert.That(scaler.matchWidthOrHeight, Is.EqualTo(0.5f).Within(0.0001f));
    }

    private static void AssertCorner(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
    {
        Assert.IsNotNull(rect);
        AssertVector(rect.anchorMin, anchor);
        AssertVector(rect.anchorMax, anchor);
        AssertVector(rect.pivot, pivot);
        AssertVector(rect.anchoredPosition, anchoredPosition);
        AssertVector(rect.sizeDelta, size);
        AssertVector(rect.localScale, Vector3.one);
    }

    private static void AssertVector(Vector2 actual, Vector2 expected)
    {
        Assert.That(Vector2.Distance(actual, expected), Is.LessThan(0.0001f), $"Expected {expected}, got {actual}");
    }

    private static void AssertVector(Vector3 actual, Vector3 expected)
    {
        Assert.That(Vector3.Distance(actual, expected), Is.LessThan(0.0001f), $"Expected {expected}, got {actual}");
    }
}
