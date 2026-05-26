using UnityEngine;
using UnityEngine.SceneManagement;

public class Sound : MonoBehaviour
{
    public float velocidade = 80f;
    public string proximaCena = "MainMenu";
    public AudioSource audioSource; // arraste o AudioSource aqui no Inspector

    private RectTransform rt;
    private bool terminando = false;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, -Screen.height);
    }

    void Update()
    {
        rt.anchoredPosition += new Vector2(0, velocidade * Time.deltaTime);

        if (rt.anchoredPosition.y > rt.sizeDelta.y + Screen.height && !terminando)
            IniciarFim();

        if (Input.anyKeyDown && !terminando)
            IniciarFim();

        // Fade out do áudio
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