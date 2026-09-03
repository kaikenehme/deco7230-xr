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
}
