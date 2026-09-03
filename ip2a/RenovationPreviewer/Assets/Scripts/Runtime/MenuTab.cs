using System;

[Flags]
public enum MenuTab
{
    None = 0,
    Paint = 1,
    Material = 2,
    Furniture = 4,     // "add furniture" — floor only
    Swap = 8,
    Remove = 16,
    KeepPrompt = 32,   // "this is staying — change it?"
}
