using UnityEngine;

public static class PortalPlayerDetector
{
    public static bool TryGetPlayer(Collider2D coll, out Player player)
    {
        player = null;

        if (coll == null)
            return false;

        player = coll.GetComponent<Player>();
        if (player == null)
            player = coll.GetComponentInParent<Player>();

        return player != null;
    }
}

