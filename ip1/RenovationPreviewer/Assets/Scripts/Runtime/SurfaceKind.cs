using System;

/// <summary>Which room surface a Surface is; drives which catalogue materials apply.</summary>
[Flags]
public enum SurfaceKind
{
    None = 0,
    Floor = 1,
    Wall = 2,
    Ceiling = 4,
    Trim = 8,
}
