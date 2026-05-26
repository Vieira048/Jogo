using PinePie.SimpleJoystick;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gerenciador de input mobile. Conectado ao PinePie Joystick.
/// </summary>
public class MobileInputManager : MonoBehaviour
{
    [Header("------Referências de UI Mobile------")]
    [Tooltip("Arraste o objeto do Joystick do PinePie aqui")]
    public JoystickController joystick;

    [Tooltip("Botão de ataque")]
    public Button attackButton;

    [Tooltip("Botão de habilidade Rage")]
    public Button rageButton;

    [Tooltip("Ícone/imagem do botão de Rage (para efeito visual)")]
    public Image rageButtonImage;

    [Header("------Cores do Botão de Rage------")]
    public Color rageReadyColor = new Color(1f, 0.5f, 0f, 1f);
    public Color rageNotReadyColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
    public float joystickTouchPadding = 32f;

    [HideInInspector] public bool attackPressed = false;
    [HideInInspector] public bool ragePressed = false;

    public static MobileInputManager instance;
    private const int NoTrackedTouch = -1;

    private static MobileInputManager previousInstance;
    private int trackedMovementTouchId = NoTrackedTouch;
    private Canvas joystickCanvas;

    public static MobileInputManager ActiveInstance
    {
        get
        {
            if (IsUsable(instance))
                return instance;

            if (IsUsable(previousInstance))
            {
                instance = previousInstance;
                previousInstance = null;
                return instance;
            }

            instance = FindActiveInstance();
            return instance;
        }
    }

    public static void RegisterActiveInstance(MobileInputManager mobileInput)
    {
        if (IsUsable(mobileInput))
        {
            if (instance != mobileInput && IsUsable(instance))
                previousInstance = instance;

            instance = mobileInput;
        }
        else if (!IsUsable(instance))
            instance = FindActiveInstance();
    }

    private void Awake()
    {
        RegisterActiveInstance(this);
    }

    private void Start()
    {
        if (attackButton != null)
            attackButton.onClick.AddListener(OnAttackPressed);

        if (rageButton != null)
            rageButton.onClick.AddListener(OnRagePressed);

        UpdateRageButtonVisual(false);
    }

    private void OnEnable()
    {
        RegisterActiveInstance(this);
    }

    private void OnDisable()
    {
        ResetRuntimeInput();

        if (instance == this)
            RestoreInstanceAfter(this);
    }

    private void OnDestroy()
    {
        if (instance == this)
            RestoreInstanceAfter(this);
    }

    private void LateUpdate()
    {
        attackPressed = false;
        ragePressed = false;

        if (GameManager.instance != null && GameManager.instance.weapon != null)
            UpdateRageButtonVisual(GameManager.instance.weapon.CanRageSkill);
    }

    private void OnAttackPressed()
    {
        attackPressed = true;
    }

    private void OnRagePressed()
    {
        ragePressed = true;
    }

    private void UpdateRageButtonVisual(bool isReady)
    {
        if (rageButtonImage == null) return;
        rageButtonImage.color = isReady ? rageReadyColor : rageNotReadyColor;
    }

    /// <summary>
    /// Retorna a direção do joystick PinePie com um fallback direto por toque.
    /// </summary>
    public Vector3 GetMovementInput()
    {
        if (joystick == null) return Vector3.zero;

        if (TryGetDirectTouchMovement(out Vector2 touchDirection))
            return new Vector3(touchDirection.x, touchDirection.y, 0f);

        Vector2 joystickDirection = joystick.InputDirection;
        return new Vector3(joystickDirection.x, joystickDirection.y, 0f);
    }

    public void ResetRuntimeInput()
    {
        attackPressed = false;
        ragePressed = false;
        trackedMovementTouchId = NoTrackedTouch;

        if (joystick != null && joystick.handle != null && joystick.snapHandleBack)
            joystick.handle.anchoredPosition = Vector2.zero;
    }

    private static bool IsUsable(MobileInputManager mobileInput)
    {
        return mobileInput != null && mobileInput.isActiveAndEnabled && mobileInput.gameObject.activeInHierarchy;
    }

    private static MobileInputManager FindActiveInstance(MobileInputManager ignoredInput = null)
    {
        MobileInputManager[] inputs = FindObjectsByType<MobileInputManager>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        for (int i = 0; i < inputs.Length; i++)
        {
            if (inputs[i] == ignoredInput)
                continue;

            if (IsUsable(inputs[i]))
                return inputs[i];
        }

        return null;
    }

    private static void RestoreInstanceAfter(MobileInputManager unavailableInput)
    {
        if (IsUsable(previousInstance) && previousInstance != unavailableInput)
        {
            instance = previousInstance;
            previousInstance = null;
            return;
        }

        instance = FindActiveInstance(unavailableInput);
        previousInstance = null;
    }

    private bool TryGetDirectTouchMovement(out Vector2 direction)
    {
        direction = Vector2.zero;

        if (joystick == null || joystick.joystickBase == null)
        {
            trackedMovementTouchId = NoTrackedTouch;
            return false;
        }

        if (Input.touchCount == 0)
        {
            trackedMovementTouchId = NoTrackedTouch;
            return false;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
            {
                if (trackedMovementTouchId == touch.fingerId)
                    trackedMovementTouchId = NoTrackedTouch;

                continue;
            }

            bool isTrackedTouch = trackedMovementTouchId == touch.fingerId;
            bool canStartTracking = trackedMovementTouchId == NoTrackedTouch && IsInsideJoystickTouchArea(touch.position);

            if (!isTrackedTouch && !canStartTracking)
                continue;

            trackedMovementTouchId = touch.fingerId;
            direction = CalculateTouchDirection(touch.position);
            return true;
        }

        trackedMovementTouchId = NoTrackedTouch;
        return false;
    }

    private bool IsInsideJoystickTouchArea(Vector2 screenPosition)
    {
        RectTransform joystickRect = joystick.GetComponent<RectTransform>();
        Camera eventCamera = GetJoystickEventCamera();

        if (joystickRect != null && RectTransformUtility.RectangleContainsScreenPoint(joystickRect, screenPosition, eventCamera))
            return true;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(joystick.joystickBase, screenPosition, eventCamera, out Vector2 localPoint))
            return false;

        return localPoint.magnitude <= GetJoystickRadius() + joystickTouchPadding;
    }

    private Vector2 CalculateTouchDirection(Vector2 screenPosition)
    {
        Camera eventCamera = GetJoystickEventCamera();

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(joystick.joystickBase, screenPosition, eventCamera, out Vector2 localPoint))
            return Vector2.zero;

        float radius = GetJoystickRadius();
        Vector2 clamped = Vector2.ClampMagnitude(localPoint, radius);
        Vector2 direction = clamped / radius;

        if (direction.magnitude < joystick.deadZone)
            direction = Vector2.zero;

        if (joystick.handle != null)
            joystick.handle.anchoredPosition = clamped;

        return direction;
    }

    private float GetJoystickRadius()
    {
        if (joystick.joystickRange > 0f)
            return joystick.joystickRange;

        Rect rect = joystick.joystickBase.rect;
        return Mathf.Max(1f, Mathf.Min(rect.width, rect.height) * 0.5f);
    }

    private Camera GetJoystickEventCamera()
    {
        if (joystickCanvas == null && joystick != null)
            joystickCanvas = joystick.GetComponentInParent<Canvas>();

        if (joystickCanvas == null || joystickCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return joystickCanvas.worldCamera;
    }
}
