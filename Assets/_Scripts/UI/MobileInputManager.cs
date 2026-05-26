using PinePie.SimpleJoystick;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gerenciador de input mobile. Conectado agora ao PinePie Joystick!
/// </summary>
public class MobileInputManager : MonoBehaviour
{
    [Header("------Referências de UI Mobile------")]
    [Tooltip("Arraste o objeto do Joystick do PinePie aqui")]
    public JoystickController joystick; // Atualizado para o tipo do PinePie

    [Tooltip("Botão de ataque")]
    public Button attackButton;

    [Tooltip("Botão de habilidade Rage")]
    public Button rageButton;

    [Tooltip("Ícone/imagem do botão de Rage (para efeito visual)")]
    public Image rageButtonImage;

    [Header("------Cores do Botão de Rage------")]
    public Color rageReadyColor = new Color(1f, 0.5f, 0f, 1f);
    public Color rageNotReadyColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);

    [HideInInspector] public bool attackPressed = false;
    [HideInInspector] public bool ragePressed = false;

    public static MobileInputManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (attackButton != null)
            attackButton.onClick.AddListener(OnAttackPressed);

        if (rageButton != null)
            rageButton.onClick.AddListener(OnRagePressed);

        UpdateRageButtonVisual(false);
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
    /// Retorna a direção do joystick PinePie.
    /// </summary>
    public Vector3 GetMovementInput()
    {
        if (joystick == null) return Vector3.zero;

        // Atualizado: Puxando o InputDirection do PinePie
        return new Vector3(joystick.InputDirection.x, joystick.InputDirection.y, 0f);
    }
}