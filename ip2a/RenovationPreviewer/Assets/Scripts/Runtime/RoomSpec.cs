/// <summary>Room envelope in metres + the XRI interaction layer used for teleport. Shared by SceneBuilder (Editor) and tests.</summary>
public static class RoomSpec
{
    public const float W = 7f, D = 5.5f, H = 2.7f;
    /// <summary>Interaction layer 31 — the XRI rig's Teleport Interactors live there; Near-Far uses layer 0.</summary>
    public const int TeleportLayer = 31;
}
