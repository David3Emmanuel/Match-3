using System.Collections;
using System.Collections.Generic;

public struct MatchResult
{
    public List<Potion> connectedPotions;
    public MatchDirection direction;
}

public enum MatchDirection
{
    Vertical,
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None,
}

public struct Node
{
    public bool isUsable;
    public Potion potion;
}