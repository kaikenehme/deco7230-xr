using UnityEngine;

/// <summary>One card of the fan deck. Touch it (other hand's controller cue, the right ray +
/// trigger, or a desktop click) to choose its colour.</summary>
public class FanChip : MonoBehaviour, ITouchable
{
    /// <summary>Closer than this to the sample = the hand holding it, which must not pick.</summary>
    public const float HoldingHandRadius = 0.08f;

    public int Index;
    public FanDeck Deck;
    public Color Colour;

    public static bool IsOtherHand(Vector3 cuePosition, Vector3 samplePosition) =>
        Vector3.Distance(cuePosition, samplePosition) > HoldingHandRadius;

    public void Touch() { if (Deck != null) Deck.Pick(Index); }

    void OnTriggerEnter(Collider other)
    {
        var cue = other.GetComponent<MarkTool>();
        if (cue == null || Deck == null) return;
        if (IsOtherHand(cue.transform.position, Deck.transform.position)) Touch();
    }
}
