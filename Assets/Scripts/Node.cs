using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector2 position;
    public Vector2 parent;

    public Node(Vector2 position, Vector2 parent)
    {
        this.position = position;
        this.parent = parent;
    }
}
