using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalFinal : MonoBehaviour
{
    [Header("------Configuracao do Portal------")]
    public string SceneName = "Creditos";

    private BoxCollider2D portalCollider;
    private ContactFilter2D contactFilter;
    private readonly Collider2D[] overlapHits = new Collider2D[8];
    private bool changingScene;

    private void Awake()
    {
        portalCollider = GetComponent<BoxCollider2D>();
        contactFilter.NoFilter();

        if (portalCollider != null)
            portalCollider.isTrigger = true;
    }

    private void Update()
    {
        ScanForPlayer();
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        TryActivate(coll);
    }

    private void ScanForPlayer()
    {
        if (changingScene || portalCollider == null || !portalCollider.enabled)
            return;

        Physics2D.SyncTransforms();

        int hitCount = portalCollider.Overlap(contactFilter, overlapHits);
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = overlapHits[i];
            overlapHits[i] = null;

            if (TryActivate(hit))
                break;
        }
    }

    public bool TryActivate(Collider2D coll)
    {
        if (changingScene || !PortalPlayerDetector.TryGetPlayer(coll, out Player player))
            return false;

        changingScene = true;

        if (GameManager.instance != null)
            GameManager.instance.SaveState();

        if (SceneName == "Creditos")
            player.gameObject.SetActive(false);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneName);
        return true;
    }
}
