using UnityEngine;

public class Portal_Door : Colliderable
{
    public BoxCollider2D boxOUT;
    public float exitOffset = 0.65f;
    public float teleportCooldown = 0.25f;
    public Vector2 minimumTriggerSize = new Vector2(0.75f, 0.45f);

    private static float nextAllowedTeleportTime;
    private BoxCollider2D portalCollider;
    private ContactFilter2D contactFilter;
    private readonly Collider2D[] overlapHits = new Collider2D[8];

    protected override void Start()
    {
        base.Start();
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

        if (portalCollider == null || !portalCollider.enabled)
            return;

        Physics2D.SyncTransforms();

        int hitCount = portalCollider.Overlap(contactFilter, overlapHits);
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = overlapHits[i];
            overlapHits[i] = null;

            if (TryTeleport(hit))
                break;
        }
    }

    public bool TryTeleport(Collider2D coll)
    {
        if (Time.time < nextAllowedTeleportTime)
            return false;

        if (boxOUT == null)
        {
            Debug.LogError($"Portal door '{name}' cannot teleport because boxOUT is missing.");
            return false;
        }

        if (!PortalPlayerDetector.TryGetPlayer(coll, out Player player))
            return false;

        Vector3 destination = boxOUT.transform.position + boxOUT.transform.up * exitOffset;
        destination.z = 0f;

        LogDebug($"teleporting player from {player.transform.position} to {destination} via '{boxOUT.name}'.");

        player.transform.position = destination;
        nextAllowedTeleportTime = Time.time + teleportCooldown;
        Physics2D.SyncTransforms();
        return true;
    }

    public void ConfigureRuntimeCollider()
    {
        portalCollider = GetComponent<BoxCollider2D>();
        contactFilter.NoFilter();

        if (portalCollider == null)
            return;

        portalCollider.isTrigger = true;

        Vector2 size = portalCollider.size;
        size.x = Mathf.Max(size.x, minimumTriggerSize.x);
        size.y = Mathf.Max(size.y, minimumTriggerSize.y);
        portalCollider.size = size;
    }

    public static void ResetTeleportCooldown()
    {
        nextAllowedTeleportTime = 0f;
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        TryTeleport(coll);
    }

    private void OnTriggerStay2D(Collider2D coll)
    {
        TryTeleport(coll);
    }

    private void LogDebug(string message)
    {
        if (Debug.isDebugBuild)
            Debug.Log($"[Portal_Door] {name}: {message}");
    }
}
