using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Room 
{
    public Coord origin;
    public IntVector2 dimensions;
    public List<Tile> tiles;

    public Room(Coord origin_, IntVector2 dimensions_)
    {
        origin = origin_;
        dimensions = dimensions_;
        tiles = new List<Tile>();
    }

    public int Left
    {
        get { return origin.x; }
    }

    public int Right
    {
        get { return origin.x + dimensions.x - 1; }
    }

    public int Bottom
    {
        get { return origin.y; }
    }

    public int Top
    {
        get { return origin.y + dimensions.y - 1; }
    }

    public Vector2 Center
    {
        get {
            return new Vector2(
          origin.x + (float)(dimensions.x - 1) / 2f,
          origin.y + (float)(dimensions.y - 1) / 2f); }
    }

    public Coord CenterCoord
    {
        get
        {
            return new Coord(origin.x + dimensions.x / 2, origin.y + dimensions.y / 2);
        }
    }

    public int Radius
    {
        get { return Mathf.Max(dimensions.x / 2, dimensions.y / 2); }
    }

    public static bool RoomsTouching(Room a, Room b)
    {
        return a.RoomsTouching(b);
    }

    public bool RoomsTouching(Room other)
    {
        Coord[] corners = new Coord[4];
        Coord bottomLeftCorner = other.origin;
        Coord bottomRightCorner = new Coord(other.Right, other.Bottom);
        Coord topLeftCorner = new Coord(other.Left, other.Top);
        Coord topRightCorner = new Coord(other.Right, other.Top);
        corners[0] = bottomLeftCorner;
        corners[1] = bottomRightCorner;
        corners[2] = topLeftCorner;
        corners[3] = topRightCorner;
        foreach (Coord corner in corners)
        {
            if (CoordContainedWithinRoom(corner)) return true;
        }
        return false;
    }

    bool CoordContainedWithinRoom(Coord coord)
    {
        return (coord.x >= Left && coord.x <= Right) &&
            (coord.y >= Bottom && coord.y <= Top);
    }

    public static float Distance(Room a, Room b)
    {
        return a.Distance(b);
    }

    public float Distance(Room other)
    {
        if (RoomsTouching(other)) return 0;
        return Vector2.Distance(Center, other.Center) - Radius - other.Radius;
    }
}
