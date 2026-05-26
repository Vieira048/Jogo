using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("------Paineis------")]
    public CharacterMenu characterMenu;
    public CharacterHUD characterHUD;
    public FloatingTextManager floatingTextManager;

    [Header("------Animacoes------")]
    public Animator deathMenuAnim;

    [Header("------Configuracoes de Cena------")]
    public GameObject controlesMobile;
    public GameObject menuObjetoPrincipal;

    [Header("------Layout Mobile------")]
    public Vector2 mobileReferenceResolution = new Vector2(1280, 720);
    [Range(0f, 1f)] public float mobileMatchWidthOrHeight = 0.5f;
    public Vector2 joystickSize = new Vector2(180, 180);
    public Vector2 joystickMargin = new Vector2(56, 48);
    public Vector2 attackButtonSize = new Vector2(160, 160);
    public Vector2 attackButtonMargin = new Vector2(48, 36);
    public Vector2 rageButtonSize = new Vector2(92, 92);
    public Vector2 rageButtonMargin = new Vector2(72, 214);
    public Vector2 menuButtonSize = new Vector2(76, 76);
    public Vector2 menuButtonMargin = new Vector2(42, 30);

    private void Start()
    {
        if (deathMenuAnim != null)
            deathMenuAnim.gameObject.SetActive(false);

        ConfigureResponsiveLayout();
        UIUpdate();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConfigureResponsiveLayout();

        if (scene.name == "Inicio" || scene.name == "Creditos")
        {
            if (controlesMobile != null) controlesMobile.SetActive(false);
            if (characterHUD != null) characterHUD.gameObject.SetActive(false);
            if (menuObjetoPrincipal != null) menuObjetoPrincipal.SetActive(false);
            Time.timeScale = 1f;
        }
        else
        {
            if (controlesMobile != null) controlesMobile.SetActive(true);
            if (characterHUD != null) characterHUD.gameObject.SetActive(true);
        }
    }

    public void ConfigureResponsiveLayout()
    {
        ConfigureCanvasScaler(GetComponent<CanvasScaler>());

        if (characterHUD != null)
        {
            ConfigureCanvasScaler(characterHUD.GetComponent<CanvasScaler>());
            EnsureSafeAreaFitter(characterHUD.gameObject);
        }

        if (controlesMobile != null)
        {
            ConfigureCanvasScaler(controlesMobile.GetComponent<CanvasScaler>());
            NormalizeOverlayRect(controlesMobile.GetComponent<RectTransform>());
            EnsureSafeAreaFitter(controlesMobile);
        }

        ConfigureMobileControlPositions();
    }

    private void ConfigureCanvasScaler(CanvasScaler scaler)
    {
        if (scaler == null)
            return;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = mobileReferenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = mobileMatchWidthOrHeight;
    }

    private void EnsureSafeAreaFitter(GameObject target)
    {
        if (target == null || target.GetComponent<SafeAreaFitter>() != null)
            return;

        target.AddComponent<SafeAreaFitter>();
    }

    private void NormalizeOverlayRect(RectTransform rectTransform)
    {
        if (rectTransform == null)
            return;

        rectTransform.localScale = Vector3.one;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
    }

    private void ConfigureMobileControlPositions()
    {
        MobileInputManager mobileInput = null;

        if (controlesMobile != null)
            mobileInput = controlesMobile.GetComponentInChildren<MobileInputManager>(true);

        if (mobileInput == null)
        {
            MobileInputManager[] inputs = FindObjectsByType<MobileInputManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (inputs.Length > 0)
                mobileInput = inputs[0];
        }

        if (mobileInput != null)
        {
            if (mobileInput.joystick != null)
                AnchorBottomLeft(mobileInput.joystick.GetComponent<RectTransform>(), joystickSize, joystickMargin);

            if (mobileInput.attackButton != null)
                AnchorBottomRight(mobileInput.attackButton.GetComponent<RectTransform>(), attackButtonSize, attackButtonMargin);

            if (mobileInput.rageButton != null)
                AnchorBottomRight(mobileInput.rageButton.GetComponent<RectTransform>(), rageButtonSize, rageButtonMargin);
        }

        RectTransform menuButton = FindChildRect(characterHUD != null ? characterHUD.transform : transform, "MenuButton");
        AnchorTopRight(menuButton, menuButtonSize, menuButtonMargin);
    }

    private RectTransform FindChildRect(Transform root, string childName)
    {
        if (root == null)
            return null;

        foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
        {
            if (rect.name == childName)
                return rect;
        }

        return null;
    }

    private void AnchorBottomLeft(RectTransform rect, Vector2 size, Vector2 margin)
    {
        ConfigureAnchoredControl(rect, Vector2.zero, Vector2.zero, margin, size);
    }

    private void AnchorBottomRight(RectTransform rect, Vector2 size, Vector2 margin)
    {
        ConfigureAnchoredControl(rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-margin.x, margin.y), size);
    }

    private void AnchorTopRight(RectTransform rect, Vector2 size, Vector2 margin)
    {
        ConfigureAnchoredControl(rect, Vector2.one, Vector2.one, new Vector2(-margin.x, -margin.y), size);
    }

    private void ConfigureAnchoredControl(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
    {
        if (rect == null)
            return;

        rect.localScale = Vector3.one;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    public void UIUpdate()
    {
        if (characterMenu != null) characterMenu.UpdateMenu();
        if (characterHUD != null) characterHUD.UpdateHUD();
    }

    public void ShowText(string msg, int fontSize, Color color, Vector3 position, Vector3 motion, float duration)
    {
        if (floatingTextManager != null)
            floatingTextManager.Show(msg, fontSize, color, position, motion, duration);
    }

    public void HideDeathAnimation()
    {
        if (deathMenuAnim != null)
        {
            deathMenuAnim.SetTrigger("Hide");
            deathMenuAnim.gameObject.SetActive(false);
        }
    }

    public void ShowDeathAnimation()
    {
        if (deathMenuAnim != null)
        {
            deathMenuAnim.gameObject.SetActive(true);
            deathMenuAnim.SetTrigger("Show");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;

        if (menuObjetoPrincipal != null)
            menuObjetoPrincipal.SetActive(false);

        if (characterHUD != null)
            characterHUD.gameObject.SetActive(false);

        if (controlesMobile != null)
            controlesMobile.SetActive(false);

        SceneManager.LoadScene("Inicio");
    }
}
