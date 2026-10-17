using UnityEngine;

/// <summary>
/// A thrown chip: flies under gravity and paints the first wall or floor it hits, with a
/// brief splash. Furniture and anything else just stops it (furniture is pull-only).
/// </summary>
public class TossedChip : MonoBehaviour
{
    public const float Lifetime = 3f, SplashSize = 0.35f, SplashFade = 0.6f;

    public Color Colour { get; private set; }
    bool done;

    public static TossedChip Launch(Vector3 position, Vector3 velocity, Color colour)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "TossedChip";
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.06f;
        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader != null) go.GetComponent<Renderer>().material = new Material(shader) { color = colour };
        var rb = go.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = true;
        rb.linearVelocity = velocity;
        var chip = go.AddComponent<TossedChip>();
        chip.Colour = colour;
        Destroy(go, Lifetime);
        return chip;
    }

    void OnCollisionEnter(Collision c)
    {
        if (done) return;
        done = true;
        var s = c.collider.GetComponentInParent<Surface>();
        if (s != null && s.IsPaintTarget)
        {
            s.Commit(Colour);
            Debug.Log("[IP2b] commit toss " + s.name);
            var contact = c.contactCount > 0 ? c.GetContact(0) : default;
            Splash(contact.point != Vector3.zero ? contact.point : transform.position, contact.normal);
        }
        Destroy(gameObject);
    }

    void Splash(Vector3 at, Vector3 normal)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "Splash";
        Destroy(go.GetComponent<Collider>());
        if (normal.sqrMagnitude < 1e-4f) normal = Vector3.up;
        go.transform.SetPositionAndRotation(at + normal * 0.01f, Quaternion.FromToRotation(Vector3.up, normal));
        go.transform.localScale = new Vector3(SplashSize, 0.002f, SplashSize);
        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader != null) go.GetComponent<Renderer>().material = new Material(shader) { color = Colour };
        Destroy(go, SplashFade);
    }
}
