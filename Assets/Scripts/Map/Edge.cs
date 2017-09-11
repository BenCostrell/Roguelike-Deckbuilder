using UnityEngine;
using System.Collections;

public class Edge 
{
    public Room a;
    public Room b;
    public Vector2 pointA
    {
        get { return a.Center; }
    }
    public Vector2 pointB
    {
        get { return b.Center; }
    }

    public Edge(Room a_, Room b_)
    {
        a = a_;
        b = b_;
    }

    public float Length
    {
        get
        {
            return Vector2.Distance(pointA, pointB);
        }
    }
}
