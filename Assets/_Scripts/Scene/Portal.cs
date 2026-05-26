using System;
using UnityEngine;

public class Portal : Colliderable
{
    public string sceneName;
    public Vector2 minimumTriggerSize = new Vector2(2.2f, 1f);

    private SceneTranslate sceneTranslate;
    private BoxCollider2D portalCollider;
    private ContactFilter2D contactFilter;
    private readonly Collider2D[] overlapHits = new Collider2D[8];
    private bool changingScene;

    protected override void Start()
    {
        base.Start();

        sceneTranslate = GetComponentInChildren<SceneTranslate>(true);
        ConfigureRuntimeCollider();
    }

    protected override void Update()
    {
        ScanForPlayer();
    }

    private void ScanForPlayer()
    {
        if (portalCollider == null)
            ConfigureRuntimeCollider();

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
        if (changingScene || !PortalPlayerDetector.TryGetPlayer(coll, out _))
            return false;

        changingScene = true;

        LogDebug($"activating scene '{sceneName}' from collider '{coll.name}'.");

        if (GameManager.instance != null)
        {
            try
            {
                GameManager.instance.SaveState();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Portal '{name}' could not save before scene transition and will continue loading '{sceneName}'. {ex.Message}");
            }
        }

        if (portalCollider != null)
            portalCollider.enabled = false;

        ChangeSceneTo(sceneName);
        return true;
    }

    public void ChangeSceneTo(string sceneName)
    {
        if (sceneTranslate == null)
            sceneTranslate = GetComponentInChildren<SceneTranslate>(true);

        if (sceneTranslate == null)
        {
            Debug.LogError($"Portal '{name}' cannot change scene because SceneTranslate is missing.");
            RestorePortal();
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError($"Portal '{name}' cannot change scene because sceneName is empty.");
            RestorePortal();
            return;
        }

        sceneTranslate.ChangeToScene(sceneName);
    }

    public void ConfigureRuntimeCollider()
    {
        portalCollider = GetComponent<BoxCollider2D>();
        contactFilter.NoFilter();

        if (portalCollider == null)
            return;

        portalCollider.enabled = true;
        portalCollider.isTrigger = true;

        Vector2 size = portalCollider.size;
        size.x = Mathf.Max(size.x, minimumTriggerSize.x);
        size.y = Mathf.Max(size.y, minimumTriggerSize.y);
        portalCollider.size = size;
    }

    private void RestorePortal()
    {
        changingScene = false;

        if (portalCollider != null)
            portalCollider.enabled = true;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        TryActivate(coll);
    }

    private void OnTriggerStay2D(Collider2D coll)
    {
        TryActivate(coll);
    }

    private void LogDebug(string message)
    {
        if (Debug.isDebugBuild)
            Debug.Log($"[Portal] {name}: {message}");
    }
}
