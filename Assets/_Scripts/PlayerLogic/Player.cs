using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Mover
{
    private SpriteRenderer spriteRenderer;      // Sprite atual do jogador
    public bool isAlive = true;                 // Verifica se o jogador está vivo

    [Header("------Sistema de Fúria (Rage)------")]
    public float rage = 0;                      // Fúria atual
    public float maxRage = 50;                  // Fúria máxima

    [Header("------Mobile------")]
    [Tooltip("Ativa o modo mobile (usa joystick virtual ao invés de teclado)")]
    public bool mobileMode = false;

    // Sistema de correção de direção de ataque
    private float temp = 0f;

    protected override void Start()
    {
        base.Start();
        GetComponent<BoxCollider2D>().enabled = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
        ImmuneTime = 0.75f;
        Player.DontDestroyOnLoad(gameObject);
        OnRageChange(0f);

        // Auto-detecta mobile se não configurado manualmente
#if UNITY_ANDROID || UNITY_IOS
        mobileMode = true;
#endif
    }

    private void FixedUpdate()
    {
        if (isAlive)
        {
            float x = 0f;
            float y = 0f;

            if (mobileMode && MobileInputManager.instance != null)
            {
                // Input Mobile: lê do joystick virtual
                Vector3 mobileDir = MobileInputManager.instance.GetMovementInput();
                x = mobileDir.x;
                y = mobileDir.y;
            }
            else
            {
                // Input Teclado: WASD e setas (original)
                x = Input.GetAxisRaw("Horizontal");
                y = Input.GetAxisRaw("Vertical");
            }

            // Verifica se a direção do jogador é a mesma do frame anterior para sincronizar a arma
            if (transform.localScale.x == temp)
                GameManager.instance.weapon.animator.SetBool("SameDirection", true);
            else
                GameManager.instance.weapon.animator.SetBool("SameDirection", false);

            temp = transform.localScale.x;

            UpdateMotor(new Vector3(x, y, 0), 1);
        }
        else
        {
            pushDirection = Vector3.zero;
        }
    }

    // Função para trocar a Skin/Sprite:
    public void SwapSprite(int SkinID)
    {
        GetComponent<SpriteRenderer>().sprite = GameManager.instance.playerSprites[SkinID];
    }

    // Função de Subir de Nível: aumenta a vida máxima e restaura a vida atual
    public void OnLevelUp()
    {
        maxHitPoint += 10;
        hitPoint = maxHitPoint;
        GameManager.instance.OnUIChange();
    }

    // Função para definir o nível do jogador (chamada apenas pelo GameManager)
    public void SetLevel(int level)
    {
        for (int i = 0; i < level; i++)
            OnLevelUp();
    }

    // Função de Dano do Jogador: reduz vida, aplica repulsão e atualiza a UI
    protected override void ReceiveDamage(Damag dmg)
    {
        if (!isAlive)
            return;

        // Se não estiver no tempo de imunidade, recebe o dano
        if (Time.time - lastImmune > ImmuneTime)
        {
            lastImmune = Time.time;
            hitPoint -= dmg.damageAmount;
            pushDirection = (transform.position - dmg.origin).normalized * dmg.pushForce;

            // Sistema de Fúria: não acumula fúria enquanto a habilidade estiver sendo usada
            if (!GameManager.instance.weapon.raging)
                OnRageChange(dmg.damageAmount);
        }

        if (hitPoint <= 0)
        {
            hitPoint = 0;
            Death();
        }

        GameManager.instance.OnUIChange();
    }

    // Sistema de acúmulo de Fúria (Rage):
    public void OnRageChange(float alter)
    {
        if (rage < maxRage)
            rage += alter;
        if (rage >= maxRage)
            rage = maxRage;

        if (rage == maxRage)
            GameManager.instance.weapon.CanRageSkill = true;
    }

    // Função de Cura: restaura vida, exibe texto animado e atualiza a UI
    public void Heal(int healingAmount)
    {
        if (hitPoint == maxHitPoint)
            return;

        hitPoint += healingAmount;
        if (hitPoint > maxHitPoint)
            hitPoint = maxHitPoint;

        GameManager.instance.ShowText("+" + healingAmount.ToString() + "hp", 25, Color.green, transform.position, Vector3.up * 30, 1.0f);
        GameManager.instance.OnUIChange();
    }

    // Função de Morte do Jogador:
    protected override void Death()
    {
        isAlive = false;
        transform.localEulerAngles = new Vector3(0, 0, 90);
        GameManager.instance.UIManager.ShowDeathAnimation();
        StartCoroutine("WaitingForRespawn");
    }

    // Função de Renascimento (Respawn):
    public void Respawn()
    {
        Heal(maxHitPoint);
        isAlive = true;
        transform.localEulerAngles = Vector3.zero;
    }

    IEnumerator WaitingForRespawn()
    {
        yield return new WaitForSeconds(6);
        GameManager.instance.Respawn();
        GameManager.instance.OnUIChange();
    }
}