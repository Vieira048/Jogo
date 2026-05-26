using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script da Arma: processa os comandos de ataque e gerencia a lógica de dano
public class Weapon : Colliderable
{
    // Parâmetros de dano para cada nível da arma:
    [Header("------Parâmetros de Dano------")]
    public int[] damagePoint = { 1, 2, 3, 4, 5, 6, 7 };                          // Dano causado pela arma
    public float[] pushForce = { 2.0f, 2.2f, 2.5f, 3.0f, 3.3f, 3.6f, 4.0f };     // Força de empurrão/repulsão da arma

    // Parâmetros de Nível da Arma:
    [Header("------Parâmetros de Nível------")]
    public int weaponLevel = 0;              // Nível atual da arma
    private SpriteRenderer SpriteRenderer;   // Componente de Sprite da arma

    // Parâmetros de Controle da Arma:
    [Header("------Parâmetros de Controle------")]
    public Animator animator;               // Componente de animação
    private float swingCoolDown = 0.4f;     // Tempo de recarga do ataque (Cooldown)
    private float lastSwing;

    // Parâmetros da Habilidade de Fúria (Rage):
    [Header("------Parâmetros da Habilidade de Fúria------")]
    public GameObject flamingSword;         // Projétil da espada flamejante
    public GameObject rageState;            // Efeito visual indicando que a habilidade está ativa
    public bool CanRageSkill = false;       // Define se a habilidade está pronta para ser usada
    public bool raging = false;             // Define se a habilidade está em uso no momento
    public float ragingTime = 4f;           // Duração da habilidade

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        rageState.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();

        if (GameManager.instance != null && GameManager.instance.player != null && GameManager.instance.player.isAlive)
        {
            MobileInputManager mobileInput = MobileInputManager.ActiveInstance;

            // --- VERIFICAÇÃO DE ATAQUE (Apenas Mobile) ---
            bool attackInput = false; // Começa falso, só o botão da UI pode mudar para true

            if (mobileInput != null && mobileInput.attackPressed)
            {
                attackInput = true;
            }

            if (attackInput)
            {
                if (Time.time - lastSwing > swingCoolDown)
                {
                    lastSwing = Time.time;

                    // Ação de ataque (aciona a animação)                
                    Swing();

                    // Lança a habilidade se estiver no modo Fúria
                    if (raging)
                    {
                        CreateFlamingSword();
                    }
                    else
                    {
                        rageState.SetActive(false);
                    }
                }
            }

            // --- VERIFICAÇÃO DO BOTÃO RAGE (Apenas Mobile) ---
            bool rageInput = false; // Começa falso, só o botão da UI pode mudar para true

            if (mobileInput != null && mobileInput.ragePressed)
            {
                rageInput = true;
            }

            // Ativa a habilidade caso os requisitos sejam atendidos e ela não esteja em uso
            if (rageInput && (!raging))
            {
                if (CanRageSkill)
                {
                    raging = true;
                    rageState.SetActive(true);
                    StartCoroutine("WaitingForRestRageSkill");
                }
            }
        }
    }

    // Função de colisão e dano da arma:
    protected override void OnCollide(Collider2D coll)
    {
        // Verifica se o objeto colidido possui a Tag Fighter (Pode receber dano)
        if (coll.CompareTag("Fighter"))
        {
            // A arma não pode causar dano ao próprio jogador
            if (coll.name == "Player")
                return;

            // Se não for o jogador, configura o dano para o inimigo
            Damag dmg = new Damag
            {
                damageAmount = damagePoint[weaponLevel],
                origin = transform.position,
                pushForce = pushForce[weaponLevel]
            };

            coll.SendMessage("ReceiveDamage", dmg);
        }
    }

    // Função para acionar o gatilho da animação de ataque (Swing)
    private void Swing()
    {
        animator.SetTrigger("Swing");
    }

    // Função para instanciar a Espada Flamejante (Projétil da Habilidade)
    private void CreateFlamingSword()
    {
        GameObject go = Instantiate(flamingSword);
    }

    // Função para evoluir a arma
    public void UpgradeWeapon()
    {
        // Aumenta o nível e atualiza o sprite correspondente
        weaponLevel++;
        SpriteRenderer.sprite = GameManager.instance.weaponSprites[weaponLevel];
    }

    // Função para definir um nível específico para a arma (usado no carregamento de save)
    public void SetWeaponLevel(int level)
    {
        if (SpriteRenderer == null)
            SpriteRenderer = GetComponent<SpriteRenderer>();

        weaponLevel = level;

        if (GameManager.instance != null && GameManager.instance.weaponSprites != null && GameManager.instance.weaponSprites.Count > weaponLevel)
            SpriteRenderer.sprite = GameManager.instance.weaponSprites[weaponLevel];
    }

    public void ResetRageState()
    {
        StopCoroutine(nameof(WaitingForRestRageSkill));
        raging = false;
        CanRageSkill = false;

        if (rageState != null)
            rageState.SetActive(false);
    }

    // Corotina que controla a duração da Habilidade de Fúria
    IEnumerator WaitingForRestRageSkill()
    {
        yield return new WaitForSeconds(ragingTime);
        raging = false;
        CanRageSkill = false;
        GameManager.instance.player.rage = 0;
        GameManager.instance.OnUIChange();
    }
}
