using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// One chip of the menu grid: rounded swatch (colour or texture) with the name
/// below, a white ring + 1.08× lift on hover and a ✓ badge when committed.
/// Emits hover / exit / click; owns no logic.
/// </summary>
public class SwatchButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public UnityEvent onHover = new();
    public UnityEvent onExit = new();
    public UnityEvent onClick = new();

    public bool IsHovered { get; private set; }
    public bool IsCommitted { get; private set; }

    Image hitArea, ring, chip, badge;
    Text label, badgeText;

    void Awake() => Build();

    public void Build()
    {
        if (chip != null) return;
        // whole cell is the raycast target so the label counts too
        hitArea = gameObject.GetComponent<Image>();
        if (hitArea == null) hitArea = gameObject.AddComponent<Image>();
        hitArea.color = new Color(0, 0, 0, 0);
        hitArea.raycastTarget = true;

        var ringRt = UiKit.Child("Ring", transform);
        ringRt.anchorMin = new Vector2(0.04f, 0.34f); ringRt.anchorMax = new Vector2(0.96f, 0.98f);
        ringRt.offsetMin = new Vector2(-4, -4); ringRt.offsetMax = new Vector2(4, 4);
        ring = UiKit.RoundedImage(ringRt, Color.white); ring.raycastTarget = false; ring.enabled = false;

        var chipRt = UiKit.Child("Chip", transform);
        chipRt.anchorMin = new Vector2(0.04f, 0.34f); chipRt.anchorMax = new Vector2(0.96f, 0.98f);
        chipRt.offsetMin = chipRt.offsetMax = Vector2.zero;
        chip = UiKit.RoundedImage(chipRt, Color.white); chip.raycastTarget = false;

        var labelRt = UiKit.Child("Label", transform);
        labelRt.anchorMin = new Vector2(0, 0); labelRt.anchorMax = new Vector2(1, 0.32f);
        labelRt.offsetMin = new Vector2(2, 0); labelRt.offsetMax = new Vector2(-2, 0);
        label = UiKit.Label(labelRt, "", 20, UiKit.Text, TextAnchor.UpperCenter, bestFit: true, minSize: 14);

        var badgeRt = UiKit.Child("Badge", transform);
        badgeRt.anchorMin = new Vector2(0.72f, 0.74f); badgeRt.anchorMax = new Vector2(0.98f, 0.98f);
        badgeRt.offsetMin = badgeRt.offsetMax = Vector2.zero;
        badge = badgeRt.gameObject.AddComponent<Image>(); badge.sprite = UiKit.Circle; badge.color = UiKit.Accent; badge.raycastTarget = false;
        badgeText = UiKit.Label(badgeRt, "✓", 22, UiKit.AccentText, TextAnchor.MiddleCenter);
        badge.enabled = false; badgeText.enabled = false;
    }

    public void Set(string text, Color tint, Texture tex)
    {
        Build();
        label.text = text;
        if (tex is Texture2D t2)
        {
            chip.sprite = Sprite.Create(t2, new Rect(0, 0, t2.width, t2.height), new Vector2(0.5f, 0.5f));
            chip.type = Image.Type.Simple; chip.preserveAspect = false;
            chip.color = Color.white;
        }
        else
        {
            chip.sprite = UiKit.Rounded; chip.type = Image.Type.Sliced;
            chip.color = tint;
        }
    }

    public void SetCommitted(bool on)
    {
        Build();
        IsCommitted = on;
        badge.enabled = on; badgeText.enabled = on;
    }

    public void OnPointerEnter(PointerEventData e) { SetHover(true); onHover.Invoke(); }
    public void OnPointerExit(PointerEventData e) { SetHover(false); onExit.Invoke(); }
    public void OnPointerClick(PointerEventData e) => onClick.Invoke();

    void SetHover(bool on)
    {
        IsHovered = on;
        ring.enabled = on;
        transform.localScale = on ? Vector3.one * 1.08f : Vector3.one;
    }
}
