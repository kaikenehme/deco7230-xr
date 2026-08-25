using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>One cell of the menu grid. Emits hover / exit / click; owns no logic.</summary>
[RequireComponent(typeof(Image))]
public class SwatchButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public UnityEvent onHover = new();
    public UnityEvent onExit = new();
    public UnityEvent onClick = new();

    Image image;
    Text label;
    Outline outline;

    void Awake() => Build();

    public void Build()
    {
        if (image != null) return;
        image = GetComponent<Image>();
        outline = gameObject.GetComponent<Outline>();
        if (outline == null) outline = gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.6f);
        outline.effectDistance = new Vector2(2, -2);

        var t = new GameObject("Label", typeof(RectTransform));
        t.transform.SetParent(transform, false);
        var rt = (RectTransform)t.transform;
        rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0.38f);
        rt.offsetMin = new Vector2(4, 4); rt.offsetMax = new Vector2(-4, 0);
        var bg = t.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.55f);
        bg.raycastTarget = false;
        var lt = new GameObject("Text", typeof(RectTransform));
        lt.transform.SetParent(t.transform, false);
        var lrt = (RectTransform)lt.transform; lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        label = lt.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 28;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;   // never truncate to nothing
        label.resizeTextForBestFit = true; label.resizeTextMinSize = 11; label.resizeTextMaxSize = 24;
    }

    public void Set(string text, Color tint, Texture tex)
    {
        Build();
        label.text = text;
        image.color = tint;
        if (tex is Texture2D t2)
        {
            image.sprite = Sprite.Create(t2, new Rect(0, 0, t2.width, t2.height), new Vector2(0.5f, 0.5f));
            image.color = Color.white;
        }
        else image.sprite = null;
    }

    public void OnPointerEnter(PointerEventData e) { outline.effectColor = Color.yellow; onHover.Invoke(); }
    public void OnPointerExit(PointerEventData e) { outline.effectColor = new Color(0, 0, 0, 0.6f); onExit.Invoke(); }
    public void OnPointerClick(PointerEventData e) => onClick.Invoke();
}
