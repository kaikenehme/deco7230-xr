using UnityEngine;

/// <summary>Scene raycast for the controller ray that ignores the player's own rig (CharacterController, controller colliders) and triggers.</summary>
public static class RayUtil
{
    static readonly RaycastHit[] Buffer = new RaycastHit[16];

    public static bool TryHit(Vector3 origin, Vector3 dir, float maxDistance, Transform ignoreRoot, out RaycastHit hit)
    {
        int n = Physics.RaycastNonAlloc(origin, dir, Buffer, maxDistance, ~0, QueryTriggerInteraction.Ignore);
        hit = default;
        float best = float.MaxValue;
        for (int i = 0; i < n; i++)
        {
            var h = Buffer[i];
            // Skip the player's own body/controllers — but the menu panel also lives under the rig
            // (child of the left controller) and must block the ray so UI clicks never hit the wall behind.
            if (ignoreRoot != null && h.collider.transform.IsChildOf(ignoreRoot) && !h.collider.CompareTag("MenuPanel")) continue;
            if (h.distance < best) { best = h.distance; hit = h; }
        }
        return best < float.MaxValue;
    }

    /// <summary>
    /// The touch-only prop under a ray, or null. Walks the hits nearest first: the rig's own colliders
    /// and triggers that aren't props are skipped; the menu panel and the first solid thing that isn't a prop block.
    /// </summary>
    public static ITouchable PickTouchable(Ray ray, Transform ignoreRoot, float maxDistance)
    {
        var hits = Physics.RaycastAll(ray, maxDistance, ~0, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var h in hits)
        {
            // The menu hangs off the left hand, inside the rig, but a click on it must not reach what's behind (as TryHit).
            if (h.collider.CompareTag("MenuPanel")) return null;
            if (ignoreRoot != null && h.collider.transform.IsChildOf(ignoreRoot)) continue;
            var t = h.collider.GetComponentInParent<ITouchable>();
            if (t != null) return t;
            if (!h.collider.isTrigger) return null;
        }
        return null;
    }
}
