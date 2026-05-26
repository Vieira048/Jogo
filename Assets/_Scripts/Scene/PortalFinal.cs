using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalFinal : MonoBehaviour
{
    [Header("------Configuração do Portal------")]
    public string SceneName = "Creditos";

    private void OnTriggerEnter2D(Collider2D coll)
    {
        // Verifica se quem encostou foi o jogador
        if (coll.name == "Player")
        {
            // Salva o jogo uma última vez, se o GameManager existir
            if (GameManager.instance != null)
            {
                GameManager.instance.SaveState();
            }

            // Se o destino for os Créditos, "apagamos" o jogador.
            // Isso impede que a câmera e os sistemas do jogo travem na tela de loading.
            if (SceneName == "Creditos")
            {
                coll.gameObject.SetActive(false);
            }

            // Garante que o jogo não está pausado
            Time.timeScale = 1f;

            // Carrega a próxima cena
            SceneManager.LoadScene(SceneName);
        }
    }
}