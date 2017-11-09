using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class AStarSearch
{
    public static float Heuristic(Tile a, Tile b)
    {
        return Vector2.Distance(
            new Vector2(a.coord.x, a.coord.y),
            new Vector2(b.coord.x, b.coord.y));
    }

    public static float Heuristic(Room a, Room b)
    {
        return Vector2.Distance(a.Center, b.Center);
    }

    public static float ShortestPathDistance(Room start, Room goal)
    {
        Dictionary<Room, Room> cameFrom = new Dictionary<Room, Room>();
        Dictionary<Room, float> costSoFar = new Dictionary<Room, float>();

        PriorityQueue<Room> frontier = new PriorityQueue<Room>();
        frontier.Enqueue(start, 0);
        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            Room current = frontier.Dequeue();
            if (current == goal) break;
            foreach (Tuple<Room, float> neighborInfo in current.neighbors)
            {
                Room next = neighborInfo.first;
                float newCost;
                newCost = costSoFar[current] + neighborInfo.second;

                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        float priority = newCost + Heuristic(next, goal);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }
        return costSoFar[goal];
    }


    public static List<Tile> ShortestPath(Tile start, Tile goal)
    {
        return ShortestPath(start, goal, false, false);
    }

    public static List<Tile> ShortestPath(Tile start, Tile goal, bool raw)
    {
        return ShortestPath(start, goal, raw, false);
    }

    public static List<Tile> ShortestPath(Tile start, Tile goal, bool raw, bool avoidTraps)
    {
        return ShortestPath(start, goal, raw, avoidTraps, false);
    }

    public static List<Tile> ShortestPath(Tile start, Tile goal, bool raw, bool avoidTraps, 
        bool monsterMovement)
    {
        return ShortestPath(start, goal, raw, avoidTraps, monsterMovement, false);
    }

    public static List<Tile> ShortestPath(Tile start, Tile goal, bool raw, bool avoidTraps,
        bool monsterMovement, bool ignoreDamageableObjects)
    {
        List<Tile> path = new List<Tile>();
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        Dictionary<Tile, float> costSoFar = new Dictionary<Tile, float>();
        Tile estimatedClosestTile = start;

        PriorityQueue<Tile> frontier = new PriorityQueue<Tile>();
        frontier.Enqueue(start, 0);
        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            Tile current = frontier.Dequeue();
            if (Heuristic(current, goal) < Heuristic(estimatedClosestTile, goal))
            {
                estimatedClosestTile = current;
            }
            if (current == goal) break;
            foreach (Tile next in current.neighbors)
            {
                if ((!next.IsImpassable(monsterMovement, ignoreDamageableObjects) && !(next.containedMapObject != null &&
                    next.containedMapObject is Trap && avoidTraps && next != goal)) || raw)
                {
                    float newCost;
                    if (raw)
                    {
                        newCost = costSoFar[current] + 1;
                    }
                    else
                    {
                        newCost = costSoFar[current] + next.movementCost;
                    }

                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        float priority = newCost + Heuristic(next, goal);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }

        }
        Tile pathNode = estimatedClosestTile;
        while (pathNode != start)
        {
            path.Add(pathNode);
            pathNode = cameFrom[pathNode];
        }

        return path;
    }

    public static List<Tile> FindAllAvailableGoals(Tile start, int movesAvailable)
    {
        return FindAllAvailableGoals(start, movesAvailable, false, false);
    }

    public static List<Tile> FindAllAvailableGoals(Tile start, int movesAvailable, bool raw)
    {
        return FindAllAvailableGoals(start, movesAvailable, raw, false);
    }

    public static List<Tile> FindAllAvailableGoals(Tile start, int movesAvailable, bool raw, 
        bool avoidTraps)
    {
        return FindAllAvailableGoals(start, movesAvailable, raw, avoidTraps, false);
    }

    public static List<Tile> FindAllAvailableGoals(Tile start, int movesAvailable, bool raw, 
        bool avoidTraps, bool monsterMovement)
    {
        return FindAllAvailableGoals(start, movesAvailable, raw, avoidTraps, 
            monsterMovement, false);
    }

    public static List<Tile> FindAllAvailableGoals(Tile start, int movesAvailable, bool raw, 
        bool avoidTraps, bool monsterMovement, bool ignoreDamageableObjects)
    {
        List<Tile> availableGoals = new List<Tile>();
        if (movesAvailable == 0) return availableGoals;
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        Dictionary<Tile, int> costSoFar = new Dictionary<Tile, int>();

        Queue<Tile> frontier = new Queue<Tile>();
        frontier.Enqueue(start);
        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            Tile current = frontier.Dequeue();
            if (costSoFar[current] <= movesAvailable)
            {
                if (current != start) availableGoals.Add(current);
                if (!(current.containedMapObject != null && current.containedMapObject is Trap
                    && avoidTraps))
                {
                    foreach (Tile next in current.neighbors)
                    {
                        if (!next.IsImpassable(monsterMovement, ignoreDamageableObjects) || raw)
                        {
                            int newCost;
                            if (raw)
                            {
                                newCost = costSoFar[current] + 1;
                            }
                            else
                            {
                                newCost = costSoFar[current] + next.movementCost;
                            }
                            if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                            {
                                costSoFar[next] = newCost;
                                frontier.Enqueue(next);
                                cameFrom[next] = current;
                            }
                        }
                    }
                }
            }
        }
        return availableGoals;
    }

}

public class PriorityQueue<T>
{
    public List<PrioritizedItem<T>> elements = new List<PrioritizedItem<T>>();

    public int Count { get { return elements.Count; } }

    public void Enqueue(T item, float priority)
    {
        elements.Add(new PrioritizedItem<T>(item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;
        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].priority < elements[bestIndex].priority) bestIndex = i;
        }

        T bestItem = elements[bestIndex].item;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }
}

public class PrioritizedItem<T>
{
    public T item;
    public float priority;
    public PrioritizedItem(T item_, float priority_)
    {
        item = item_;
        priority = priority_;
    }
}
