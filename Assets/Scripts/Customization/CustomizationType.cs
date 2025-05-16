using System;

[Flags]
public enum CustomizationType
{
    Hairs1 = 1,
    Hairs2 = 2,
    Hairs3 = 4,
    Down = 8,
    Top = 16,
    TopBody = 32,
    DownBody = 64,
    Hairs = 1 | 2 | 4,
}