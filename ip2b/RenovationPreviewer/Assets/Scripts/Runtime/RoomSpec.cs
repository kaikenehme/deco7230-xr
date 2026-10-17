/// <summary>Room envelope in metres + the XRI interaction layer used for teleport. Shared by SceneBuilder (Editor) and tests.</summary>
public static class RoomSpec
{
    public const float W = 5.5f, D = 5.0f, H = 2.7f;   // IP2b: a real living-room size (was 9 x 7 x 2.9)
    /// <summary>Interaction layer 31 — the XRI rig's Teleport Interactors live there; Near-Far uses layer 0.</summary>
    public const int TeleportLayer = 31;
}
