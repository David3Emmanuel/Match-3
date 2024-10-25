using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool isUsable;
    public Potion potion;

    public Node(bool isUsable, Potion potion)
    {
        this.isUsable = isUsable;
        this.potion = potion;
    }
}
