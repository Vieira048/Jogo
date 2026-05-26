using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsScroller : MonoBehaviour
{
    [Header("Créditos")]
    public float velocidade = 80f;
    public string proximaCena = "MainMenu";

    [Header("Áudio")]
    public float tempoInicioMusica = 0f;

    private RectTransform rt;
    private AudioSource audioSource;
    private bool terminando = false;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, -Screen.height);

        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            Debug.Log("AudioSource encontrado! Tocando...");
            audioSource.time = tempoInicioMusica;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("AudioSource NAO encontrado no objeto!");
        }
    }

    void Update()
    {
        rt.anchoredPosition += new Vector2(0, velocidade * Time.deltaTime);

        if (rt.anchoredPosition.y > rt.sizeDelta.y + Screen.height * 2f && !terminando)
            IniciarFim();

        if (Input.anyKeyDown && !terminando)
            IniciarFim();

        if (terminando && audioSource != null)
        {
            audioSource.volume -= Time.deltaTime * 0.5f;
            if (audioSource.volume <= 0)
                SceneManager.LoadScene(proximaCena);
        }
    }

    void IniciarFim()
    {
        terminando = true;
    }
}